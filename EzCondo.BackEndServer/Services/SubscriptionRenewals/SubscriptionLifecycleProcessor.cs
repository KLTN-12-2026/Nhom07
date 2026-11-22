using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Infrastructure.Email;
using EzCondo.BackEndServer.Infrastructure.Subscriptions;
using EzCondo.BackEndServer.Infrastructure.Subscriptions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EzCondo.BackEndServer.Services.SubscriptionRenewals
{
	public sealed class SubscriptionLifecycleProcessor(
		ApplicationDbContext dbcontext,
		TimeProvider timeProvider,
		IOptions<SubscriptionLifecycleOptions> optionsAccessor,
		ISubscriptionExpiryNoticeSender noticeSender,
		ITenantBackgroundJobCoordinator jobCoordinator,
		ILogger<SubscriptionLifecycleProcessor> logger)
		: ISubscriptionLifecycleProcessor
	{
		private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);

		public async Task<SubscriptionLifecycleRunResult> ProcessAsync(
			CancellationToken cancellationToken = default)
		{
			var now = timeProvider.GetUtcNow();

			var options = optionsAccessor.Value;

			var vietnamNow = now.ToOffset(VietnamOffset);

			var today = DateOnly.FromDateTime(vietnamNow.DateTime);

			var candidates = await dbcontext.SubscriptionRenewalRequests
				.AsNoTracking()
				.Where(
					x =>
						(x.Status == SubscriptionRenewalRequestStatus.TenantDeclined ||
						x.Status == SubscriptionRenewalRequestStatus.AwaitingTenantDecision)
						&&
						(x.Subscription.Status == SubscriptionStatusEnum.Trial ||
						x.Subscription.Status == SubscriptionStatusEnum.Active)
						&&
						x.Subscription.EndsAt <= today.AddDays(2)
						&&
						(x.Tenant.Status == TenantStatusEnum.Trial ||
						x.Tenant.Status == TenantStatusEnum.Active))
				.Select(x => new LifecycleCandidate(
					x.Id,
					x.TenantId,
					x.SubscriptionId,
					x.Status,
					x.CurrentEndsAtSnapshot,
					x.ExpiryNoticeSentAt,
					x.ExpiryNoticeLastAttemptAt,
					x.Tenant.LegalName,
					x.Subscription.Plan.Name,
					x.Subscription.Status,
					x.Subscription.EndsAt))
				.ToListAsync(cancellationToken);

			var noticeSentCount = 0;
			var noticeFailedCount = 0;
			var expiredCount = 0;

			foreach(var candidate in candidates)
			{
				try
				{
					cancellationToken.ThrowIfCancellationRequested();

					var accessExpiresAt = SubscriptionLifecycleClock.ToExpiryInstant(
						candidate.SubscriptionEndsAt);

					var noticeAt = accessExpiresAt.AddHours(-options.ExpiryNoticeLeadHours);

					if (candidate.ExpiryNoticeSentAt is null &&
						now >= noticeAt &&
						now < accessExpiresAt)
					{
						var sent = await TrySentNoticeAsync(
							candidate,
							accessExpiresAt,
							now,
							options,
							cancellationToken);

						if (sent)
						{
							noticeSentCount++;
						}
						else
						{
							noticeFailedCount++;
						}
					}

					if (now >= accessExpiresAt)
					{
						var expired = await ExpireAsync(
							candidate,
							now,
							cancellationToken);

						if (expired)
						{
							expiredCount++;
						}
					}
				}
				catch(OperationCanceledException) when (cancellationToken.IsCancellationRequested)
				{
					throw;
				}
				catch(Exception exception)
				{
					logger.LogError(
						exception,
						"Xử lý lifecycle thất bại cho tenant {TenantId}.",
						candidate.TenantId);

					continue;
				}

				
			}

			return new SubscriptionLifecycleRunResult(
				candidates.Count,
				noticeSentCount,
				noticeFailedCount,
				expiredCount);
		}

		private async Task<bool> TrySentNoticeAsync(
			LifecycleCandidate candidate,
			DateTimeOffset accessExpiresAt,
			DateTimeOffset now,
			SubscriptionLifecycleOptions options,
			CancellationToken cancellationToken)
		{
			var retryBefore = now.AddMinutes(-options.RetryAfterMinutes);

			var claimded = await dbcontext.SubscriptionRenewalRequests
				.Where(
					x =>
						x.Id == candidate.RequestId &&
						x.ExpiryNoticeSentAt == null &&
						(
							x.ExpiryNoticeLastAttemptAt == null ||
							x.ExpiryNoticeLastAttemptAt < retryBefore
						))
				.ExecuteUpdateAsync(
					setters => setters
						.SetProperty(
							x => x.ExpiryNoticeLastAttemptAt,
							(DateTimeOffset?)now),
					cancellationToken);

			if(claimded == 0)
			{
				return false;
			}

			var admin = await dbcontext.UserRoles
				.AsNoTracking()
				.Where(
					x =>
						x.TenantId == candidate.TenantId &&
						x.Scope == RoleScopeEnum.Tenant &&
						x.RevokedAt == null &&
						x.Role.Scope == RoleScopeEnum.Tenant &&
						x.Role.TenantId == candidate.TenantId &&
						x.Role.Code == DbInitializer.TenantAdminRoleCode &&
						!x.Role.IsDeleted &&
						!x.User.IsDeleted &&
						x.User.Status == UserStatusEnum.Active &&
						x.User.EmailConfirmed &&
						x.User.Email != null &&
						x.User.Email != "")
				.Select(
					x =>
						new
						{
							Email = x.User.Email!,
							x.User.FullName
						})
				.FirstOrDefaultAsync(cancellationToken);

			if(admin is null)
			{
				await SaveNoticeErrorAsync(
					candidate.RequestId,
					"Không tìm thấy Tenant Admin đang hoạt động.",
					cancellationToken);

				return false;
			}

			try
			{
				await noticeSender.SendAsync(
					new SubscriptionExpiryNoticeMessage(
						admin.Email,
						admin.FullName,
						candidate.TenantName,
						candidate.PlanName,
						candidate.SubscriptionEndsAt,
						accessExpiresAt),
					cancellationToken);

				await dbcontext.SubscriptionRenewalRequests
					.Where(x => x.Id == candidate.RequestId)
					.ExecuteUpdateAsync(
						setters => setters
							.SetProperty(
								x => x.ExpiryNoticeSentAt,
								(DateTimeOffset?)now)
							.SetProperty(
								x => x.ExpiryNoticeLastError,
								(string?)null)
							.SetProperty(
								x => x.UpdatedAt,
								(DateTimeOffset?)now),
						cancellationToken);

				return true;
			}
			catch(Exception exception)
			{
				logger.LogError(
					exception,
					"Gửi thông báo hết hạn thất bại cho request {RequestId}.",
					candidate.RequestId);

				await SaveNoticeErrorAsync(
					candidate.RequestId,
					exception.Message,
					cancellationToken);

				return false;
			}
		}

		private async Task SaveNoticeErrorAsync(
			Guid requestId,
			string error,
			CancellationToken cancellationToken)
		{
			var truncatedError = error.Length <= 1000
				? error
				: error.Substring(0, 1000);

			await dbcontext.SubscriptionRenewalRequests
				.Where(x => x.Id == requestId)
				.ExecuteUpdateAsync(
					setters => setters
						.SetProperty(
							x => x.ExpiryNoticeLastError,
							truncatedError)
						.SetProperty(
							x => x.UpdatedAt,
							timeProvider.GetUtcNow()),
					cancellationToken);
		}

		private async Task<bool> ExpireAsync(
			LifecycleCandidate candidate,
			DateTimeOffset now,
			CancellationToken cancellationToken)
		{
			await using var transaction = await dbcontext.Database
				.BeginTransactionAsync(cancellationToken);
				
			var requestRows = await dbcontext.SubscriptionRenewalRequests
				.Where(
					x =>
						x.Id == candidate.RequestId &&
						x.TenantId == candidate.TenantId &&
						(
							x.Status ==
								SubscriptionRenewalRequestStatus.TenantDeclined ||
							x.Status ==
								SubscriptionRenewalRequestStatus.AwaitingTenantDecision)
						&&
						x.CurrentEndsAtSnapshot == candidate.CurrentEndsAtSnapshot)
				.ExecuteUpdateAsync(
					setters => setters
						.SetProperty(
							x => x.Status,
							SubscriptionRenewalRequestStatus.Expired)
						.SetProperty(
							x => x.UpdatedAt,
							(DateTimeOffset?)now),
					cancellationToken);

			if(requestRows == 0)
			{
				await transaction.RollbackAsync(cancellationToken);

				return false;
			}

			var subscriptionRows = await dbcontext.Subscriptions
				.Where(
					x =>
						x.Id == candidate.SubscriptionId &&
						x.TenantId == candidate.TenantId &&
						x.EndsAt == candidate.CurrentEndsAtSnapshot &&
						(
							x.Status == SubscriptionStatusEnum.Trial ||
							x.Status == SubscriptionStatusEnum.Active
						))
				.ExecuteUpdateAsync(
					setters => setters
						.SetProperty(
							x => x.Status,
							SubscriptionStatusEnum.Expired),
					cancellationToken);

			if(subscriptionRows != 1)
			{
				throw new InvalidOperationException(
					"Subscription không còn ở trạng thái hợp lệ để hết hạn.");
			}

			var tenantRows = await dbcontext.Tenants
				.Where(
					x =>
						x.Id == candidate.TenantId &&
						(
							x.Status == TenantStatusEnum.Trial ||
							x.Status == TenantStatusEnum.Active
						))
				.ExecuteUpdateAsync(
					setters => setters
						.SetProperty(
							x => x.Status,
							TenantStatusEnum.Expired)
						.SetProperty(
							x => x.ExpiredAt,
							(DateTimeOffset?)now)
						.SetProperty(
							x => x.ExpirationReasonCode,
							"subscription_expired")
						.SetProperty(
							x => x.UpdatedAt,
							(DateTimeOffset?)now),
					cancellationToken);

			if(tenantRows != 1)
			{
				throw new InvalidOperationException(
					"Tenant không còn ở trạng thái hợp lệ để hết hạn.");
			}

			await SoftDeleteTenantDataAsync(
				candidate.TenantId,
				now,
				cancellationToken);

			await InvalidateTenantTokenAsync(
				candidate.TenantId,
				cancellationToken);

			await transaction.CommitAsync(cancellationToken);

			jobCoordinator.Stop(candidate.TenantId);

			return true;
		}

		private async Task SoftDeleteTenantDataAsync(
			Guid tenantId,
			DateTimeOffset deletedAt,
			CancellationToken cancellationToken)
		{
			await dbcontext.Users
				.Where(x => x.TenantId == tenantId && !x.IsDeleted)
				.ExecuteUpdateAsync(
					setters => setters
						.SetProperty(x => x.IsDeleted, true)
						.SetProperty(x => x.DeletedAt, (DateTimeOffset?)deletedAt)
						.SetProperty(x => x.UpdatedAt, (DateTimeOffset?)deletedAt),
					cancellationToken);

			await dbcontext.Apartments
				.Where(x => x.TenantId == tenantId && !x.IsDeleted)
				.ExecuteUpdateAsync(
					setters => setters
						.SetProperty(x => x.IsDeleted, true)
						.SetProperty(x => x.DeletedAt, (DateTimeOffset?)deletedAt)
						.SetProperty(x => x.UpdatedAt, (DateTimeOffset?)deletedAt),
					cancellationToken);

			await dbcontext.Contracts
				.Where(x => x.TenantId == tenantId && !x.IsDeleted)
				.ExecuteUpdateAsync(
					setters => setters
						.SetProperty(x => x.IsDeleted, true)
						.SetProperty(x => x.DeletedAt, (DateTimeOffset?)deletedAt)
						.SetProperty(x => x.UpdatedAt, (DateTimeOffset?)deletedAt),
					cancellationToken);

			await dbcontext.Complaints
				.Where(x => x.TenantId == tenantId && !x.IsDeleted)
				.ExecuteUpdateAsync(
					setters => setters
						.SetProperty(x => x.IsDeleted, true)
						.SetProperty(x => x.DeletedAt, (DateTimeOffset?)deletedAt)
						.SetProperty(x => x.UpdatedAt, (DateTimeOffset?)deletedAt),
					cancellationToken);

			await dbcontext.Invitations
				.Where(x => x.TenantId == tenantId && !x.IsDeleted)
				.ExecuteUpdateAsync(
					setters => setters
						.SetProperty(x => x.IsDeleted, true)
						.SetProperty(x => x.DeletedAt, (DateTimeOffset?)deletedAt),
					cancellationToken);

			await dbcontext.Invoices
				.Where(x => x.TenantId == tenantId && !x.IsDeleted)
				.ExecuteUpdateAsync(
					setters => setters
						.SetProperty(x => x.IsDeleted, true)
						.SetProperty(x => x.DeletedAt, (DateTimeOffset?)deletedAt)
						.SetProperty(x => x.UpdatedAt, (DateTimeOffset?)deletedAt),
					cancellationToken);

			await dbcontext.Payrolls
				.Where(x => x.TenantId == tenantId && !x.IsDeleted)
				.ExecuteUpdateAsync(
					setters => setters
						.SetProperty(x => x.IsDeleted, true)
						.SetProperty(x => x.DeletedAt, (DateTimeOffset?)deletedAt)
						.SetProperty(x => x.UpdatedAt, (DateTimeOffset?)deletedAt),
					cancellationToken);

			await dbcontext.Services
				.Where(x => x.TenantId == tenantId && !x.IsDeleted)
				.ExecuteUpdateAsync(
					setters => setters
						.SetProperty(x => x.IsDeleted, true)
						.SetProperty(x => x.DeletedAt, (DateTimeOffset?)deletedAt)
						.SetProperty(x => x.UpdatedAt, (DateTimeOffset?)deletedAt),
					cancellationToken);

			await dbcontext.StaffTimeSheets
				.Where(x => x.TenantId == tenantId && !x.IsDeleted)
				.ExecuteUpdateAsync(
					setters => setters
						.SetProperty(x => x.IsDeleted, true)
						.SetProperty(x => x.DeletedAt, (DateTimeOffset?)deletedAt)
						.SetProperty(x => x.UpdatedAt, (DateTimeOffset?)deletedAt),
					cancellationToken);

			await dbcontext.SubscriptionInvoices
				.Where(x => x.TenantId == tenantId && !x.IsDeleted)
				.ExecuteUpdateAsync(
					setters => setters
						.SetProperty(x => x.IsDeleted, true)
						.SetProperty(x => x.DeletedAt, (DateTimeOffset?)deletedAt)
						.SetProperty(x => x.UpdatedAt, (DateTimeOffset?)deletedAt),
					cancellationToken);
		}

		private async Task InvalidateTenantTokenAsync(
			Guid tenantId,
			CancellationToken cancellationToken)
		{
			var securityStamp = Guid.NewGuid().ToString("N");

			await dbcontext.Users
				.Where(
					x =>
						x.TenantId == tenantId &&
						!x.IsDeleted)
				.ExecuteUpdateAsync(
					setters => setters
						.SetProperty(
							x => x.SecurityStamp,
							securityStamp)
						.SetProperty(
							x => x.UpdatedAt,
							timeProvider.GetUtcNow()),
					cancellationToken);
		}

		private sealed record LifecycleCandidate(
			Guid RequestId,
			Guid TenantId,
			Guid SubscriptionId,
			SubscriptionRenewalRequestStatus Status,
			DateOnly CurrentEndsAtSnapshot,
			DateTimeOffset? ExpiryNoticeSentAt,
			DateTimeOffset? ExpiryNoticeLastAttemptAt,
			string TenantName,
			string PlanName,
			SubscriptionStatusEnum SubscriptionStatus,
			DateOnly SubscriptionEndsAt);
	}
}
