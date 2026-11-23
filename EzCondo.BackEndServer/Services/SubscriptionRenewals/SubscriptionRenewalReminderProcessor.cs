using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Infrastructure.Subscriptions.Options;
using EzCondo.BackEndServer.Infrastructure.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace EzCondo.BackEndServer.Services.SubscriptionRenewals
{
	public sealed class SubscriptionRenewalReminderProcessor(
		ApplicationDbContext dbcontext,
		TimeProvider timeProvider,
		IOptions<SubscriptionRenewalReminderOptions> optionsAccessor,
		ITenantBackgroundJobCoordinator jobCoordinator,
		ILogger<SubscriptionRenewalReminderProcessor> logger)
		: ISubscriptionRenewalReminderProcessor
	{
		private const string RenewalPeriodUniqueIndex = 
			"ux_subscription_renewal_requests_period";

		private static readonly TimeSpan VietnamUtcOffset = TimeSpan.FromHours(7);

		public async Task<SubscriptionRenewalReminderRunResult> ProcessAsync(
			CancellationToken cancellationToken = default)
		{
			var options = optionsAccessor.Value;

			var now = timeProvider.GetUtcNow();
			
			var vietnamNow = now.ToOffset(VietnamUtcOffset);

			var today = DateOnly.FromDateTime(vietnamNow.DateTime);

			var lookupUntil = today
				.AddMonths(options.PaidReminderMonths)
				.AddDays(7);

			var potentialSubscriptions =
				await dbcontext.Subscriptions
					.AsNoTracking()
					.Where(x =>
						(
							x.Status == SubscriptionStatusEnum.Trial ||
							x.Status == SubscriptionStatusEnum.Active)
						&&
						x.EndsAt > today &&
						x.EndsAt <= lookupUntil)
					.Select(x => new
					{
						SubscriptionId = x.Id,
						x.TenantId,
						SubscriptionStatus = x.Status,
						x.EndsAt,
						TenantStatus = x.Tenant.Status,
						TenantCode = x.Tenant.Code
					})
					.ToListAsync(cancellationToken);

			var eligibleSubscriptions = new List<RenewalCandidate>();

			var skippedStateCount = 0;

			foreach(var subscription in potentialSubscriptions)
			{
				if(jobCoordinator.IsStopped(subscription.TenantId))
				{
					skippedStateCount++;

					logger.LogInformation(
						"Bỏ qua reminder của tenant {TenantCode} vì job tenant đã bị dừng.",
						subscription.TenantCode);

					continue;
				}

				var reminder = subscription.SubscriptionStatus switch
				{
					SubscriptionStatusEnum.Trial =>
						subscription.EndsAt.AddDays(
							-options.TrialReminderDays),

					SubscriptionStatusEnum.Active =>
						subscription.EndsAt.AddMonths(
							-options.PaidReminderMonths),

					_ => throw new InvalidOperationException(
						"subscription không thuộc trạng thái được worker hỗ trợ.")
				};

				if(today < reminder)
				{
					continue;
				}

				if(subscription.TenantStatus == TenantStatusEnum.Suspended)
				{
					skippedStateCount++;

					logger.LogInformation(
						"Bỏ qua reminder của tenant {TenantCode} vì tenant đang bị tạm ngưng.",
						subscription.TenantCode);

					continue;
				}

				var expectedTenantStatus = 
						subscription.SubscriptionStatus ==
						SubscriptionStatusEnum.Trial
						? TenantStatusEnum.Trial
						: TenantStatusEnum.Active;

				if (subscription.TenantStatus != expectedTenantStatus)
				{
					skippedStateCount++;

					logger.LogWarning(
						"Bỏ qua reminder do trạng thái không đồng bộ." +
						" Tenant {TenantCode}: {TenantStatus}; " +
						"Subscription {SubscriptionId}: " +
						"{SubscriptionStatus}.",
						subscription.TenantCode,
						subscription.TenantStatus,
						subscription.SubscriptionId,
						subscription.SubscriptionStatus);

					continue;
				}

				eligibleSubscriptions.Add(
					new RenewalCandidate(
						subscription.SubscriptionId,
						subscription.TenantId,
						subscription.EndsAt,
						subscription.TenantCode));
			}

			if(eligibleSubscriptions.Count == 0)
			{
				return new(
					potentialSubscriptions.Count,
					0,
					0,
					0,
					skippedStateCount,
					0);
			}

			var subscriptionIds = eligibleSubscriptions
				.Select(x => x.SubscriptionId)
				.ToArray();

			var existingRequests =
				await dbcontext.SubscriptionRenewalRequests
					.AsNoTracking()
					.Where(x =>
						subscriptionIds.Contains(
							x.SubscriptionId))
					.Select(x => new
					{
						x.SubscriptionId,
						x.CurrentEndsAtSnapshot
					})
					.ToListAsync(cancellationToken);

			var existingKeys = existingRequests
				.Select(x => (
					x.SubscriptionId,
					x.CurrentEndsAtSnapshot))
				.ToHashSet();

			var candidatesToCreate = eligibleSubscriptions
				.Where(x =>
					!existingKeys.Contains((
						x.SubscriptionId,
						x.EndsAt)))
				.ToList();

			var alreadyExistsCount =
				eligibleSubscriptions.Count - candidatesToCreate.Count;

			if(candidatesToCreate.Count == 0)
			{
				return new(
					potentialSubscriptions.Count,
					eligibleSubscriptions.Count,
					0,
					alreadyExistsCount,
					skippedStateCount,
					0);
			}

			var candidateTenantIds = candidatesToCreate
				.Select(x => x.TenantId)
				.Distinct()
				.ToArray();

			var tenantsWithActiveAdminIds =
				await dbcontext.UserRoles
					.AsNoTracking()
					.Where(x =>
						x.Scope == RoleScopeEnum.Tenant &&
						x.TenantId.HasValue &&
						candidateTenantIds.Contains(x.TenantId.Value) &&
						x.RevokedAt == null &&
						!x.Role.IsDeleted &&
						x.Role.Code == DbInitializer.TenantAdminRoleCode &&
						x.Role.TenantId == x.TenantId &&
						!x.User.IsDeleted &&
						x.User.Status == UserStatusEnum.Active &&
						x.User.TenantId == x.TenantId)
					.Select(x => x.TenantId!.Value)
					.Distinct()
					.ToListAsync(cancellationToken);

			var tenantsWithActiveAdmin = tenantsWithActiveAdminIds.ToHashSet();

			var createdCount = 0;

			var missingTenantAdminCount = 0;

			foreach(var candidate in candidatesToCreate)
			{
				var renewalRequest = new SubscriptionRenewalRequest
				{
					Id = Guid.NewGuid(),
					TenantId = candidate.TenantId,
					SubscriptionId = candidate.SubscriptionId,
					CurrentEndsAtSnapshot = candidate.EndsAt,
					Status = SubscriptionRenewalRequestStatus.AwaitingTenantDecision,
					CreatedAt = now
				};

				dbcontext.SubscriptionRenewalRequests.Add(renewalRequest);

				try
				{
					await dbcontext.SaveChangesAsync(cancellationToken);

					createdCount++;
				}
				catch(DbUpdateException exception)
					when (IsDuplicatePeriod(exception))
				{
					dbcontext.Entry(renewalRequest).State = EntityState.Detached;

					alreadyExistsCount++;

					continue;
				}

				if (!tenantsWithActiveAdmin.Contains(candidate.TenantId))
				{
					missingTenantAdminCount++;

					logger.LogWarning(
						"Đã tạo yêu cầu gia hạn cho tenant " +
						"{TenantCode}, nhưng tenant chưa có " +
						"TENANT_ADMIN đang hoạt động.",
						candidate.TenantCode);
				}
			}

			return new(
				potentialSubscriptions.Count,
				eligibleSubscriptions.Count,
				createdCount,
				alreadyExistsCount,
				skippedStateCount,
				missingTenantAdminCount);
		}

		private static bool IsDuplicatePeriod(DbUpdateException exception)
		{
			return exception.InnerException is PostgresException
			{
				SqlState: PostgresErrorCodes.UniqueViolation,
				ConstraintName: RenewalPeriodUniqueIndex
			};
		}

		private sealed record RenewalCandidate(
			Guid SubscriptionId,
			Guid TenantId,
			DateOnly EndsAt,
			string TenantCode
			);
	}
}
