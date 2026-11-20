using EzCondo.BackEndServer.Contracts.SubscriptionRenewals;
using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace EzCondo.BackEndServer.Services.SubscriptionRenewals
{
	public sealed class TenantSubscriptionRenewalService(
		ApplicationDbContext dbcontext,
		IHttpContextAccessor httpContextAccessor,
		TimeProvider timeProvider)
		: ITenantSubscriptionRenewalService
	{
		public async Task<GetCurrentSubscriptionRenewalResult> GetCurrentAsync(
			CancellationToken cancellationToken = default)
		{
			if(!TryGetCurrentIdentity(
				out var userId,
				out var tenantId))
			{
				return GetCurrentSubscriptionRenewalResult.Failed(
					GetCurrentSubscriptionRenewalFailure.InvalidIdentity);
			}

			var isTenantAdmin = await IsTenantAdminAsync(
				userId,
				tenantId,
				cancellationToken);

			if (!isTenantAdmin)
			{
				return GetCurrentSubscriptionRenewalResult.Failed(
					GetCurrentSubscriptionRenewalFailure.TenantAdminRequired);
			}

			var renewal = await dbcontext.SubscriptionRenewalRequests
				.AsNoTracking()
				.Where(x =>
					x.TenantId == tenantId &&
					x.Status == SubscriptionRenewalRequestStatus.AwaitingTenantDecision)
				.OrderByDescending(x => x.CreatedAt)
				.Select(x => new RenewalRow(
					x.Id,
					x.CreatedAt,
					x.CurrentEndsAtSnapshot,
					x.Subscription.Id,
					x.Subscription.Status,
					x.Subscription.PlanId,
					x.Subscription.Plan.Name,
					x.Subscription.BillingCycleMonthsSnapshot,
					x.Subscription.StartsAt,
					x.Subscription.EndsAt))
				.FirstOrDefaultAsync(cancellationToken);

			if(renewal is null)
			{
				return GetCurrentSubscriptionRenewalResult.Failed(
					GetCurrentSubscriptionRenewalFailure.RenewalRequestNotFound);
			}

			if(renewal.SubscriptionEndsAt != renewal.CurrentEndsAtSnapshot ||
				renewal.SubscriptionStatus is not (
					SubscriptionStatusEnum.Trial or
					SubscriptionStatusEnum.Active))
			{
				return GetCurrentSubscriptionRenewalResult.Failed(
					GetCurrentSubscriptionRenewalFailure.RenewalRequestStale);
			}

			var planRows = await dbcontext.SubscriptionPlans
				.AsNoTracking()
				.Where(x =>
					!x.IsDeleted &&
					x.IsAvailable &&
					x.Price > 0 &&
					(
						x.BillingCycleMonths == 6 ||
						x.BillingCycleMonths == 12 ||
						x.BillingCycleMonths == 24
					))
				.OrderBy(x => x.Price)
				.ThenBy(x => x.Name)
				.Select(x => new PlanRow(
					x.Id,
					x.Name,
					x.Description,
					x.BillingCycleMonths,
					x.Price,
					x.FeatureLimits))
				.ToListAsync(cancellationToken);

			var availablePlans = planRows
				.Select(x =>
					new SubscriptionRenewalPlanOptionResponse(
						x.Id,
						x.Name,
						x.Description,
						x.BillingCycleMonths,
						x.Price,
						"VND",
						ParseFeatureLimits(x.FeatureLimits)))
				.ToArray();

			var response = new CurrentSubscriptionRenewalResponse(
				renewal.RequestId,
				"awaiting_tenant_decision",
				renewal.CreatedAt,
				new CurrentSubscriptionSummaryResponse(
					renewal.SubscriptionId,
					renewal.CurrentPlanId,
					renewal.CurrentPlanName,
					renewal.CurrentBillingCycleMonths,
					renewal.SubscriptionStartsAt,
					renewal.SubscriptionEndsAt),
				renewal.CurrentEndsAtSnapshot,
				availablePlans);

			return GetCurrentSubscriptionRenewalResult.Success(response);
		}

		public async Task<ConfirmSubscriptionRenewalResult> ConfirmAsync(
			Guid requestId,
			ConfirmSubscriptionRenewalRequest request,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(request);

			if(!TryGetCurrentIdentity(
				out var userId,
				out var tenantId))
			{
				return ConfirmSubscriptionRenewalResult.Failed(
					ConfirmSubscriptionRenewalFailure.InvalidIdentity);
			}

			var isTenantAdmin = await IsTenantAdminAsync(
				userId,
				tenantId,
				cancellationToken);

			if (!isTenantAdmin)
			{
				return ConfirmSubscriptionRenewalResult.Failed(
					ConfirmSubscriptionRenewalFailure.TenantAdminRequired);
			}

			var renewal = await dbcontext.SubscriptionRenewalRequests
				.AsNoTracking()
				.Where(x =>
					x.Id == requestId &&
					x.TenantId == tenantId)
				.Select(x => new ConfirmRenewalRow(
					x.Status,
					x.CurrentEndsAtSnapshot,
					x.Subscription.Status,
					x.Subscription.EndsAt))
				.SingleOrDefaultAsync(cancellationToken);

			if(renewal is null)
			{
				return ConfirmSubscriptionRenewalResult.Failed(
					ConfirmSubscriptionRenewalFailure.RenewalRequestNotFound);
			}

			if(renewal.Status != SubscriptionRenewalRequestStatus.AwaitingTenantDecision)
			{
				return ConfirmSubscriptionRenewalResult.Failed(
					ConfirmSubscriptionRenewalFailure.RenewalRequestAlreadyResponded);
			}

			if(renewal.SubscriptionEndsAt != renewal.CurrentEndsAtSnapshot ||
				renewal.SubscriptionStatus is not (
					SubscriptionStatusEnum.Trial or
					SubscriptionStatusEnum.Active))
			{
				return ConfirmSubscriptionRenewalResult.Failed(
					ConfirmSubscriptionRenewalFailure.RenewalRequestStale);
			}

			var plan = await dbcontext.SubscriptionPlans
				.AsNoTracking()
				.Where(x => x.Id == request.SelectedPlanId)
				.Select(x => new SelectedPlanNow(
					x.Id,
					x.Name,
					x.BillingCycleMonths,
					x.Price,
					x.IsAvailable,
					x.IsDeleted))
				.SingleOrDefaultAsync(cancellationToken);

			if(plan is null || plan.IsDeleted)
			{
				return ConfirmSubscriptionRenewalResult.Failed(
					ConfirmSubscriptionRenewalFailure.SubscriptionPlanNotFound);
			}

			if(!plan.IsAvailable ||
				plan.Price <= 0 ||
				!IsSupportedBillingCycle(
					plan.BillingCycleMonths))
			{
				return ConfirmSubscriptionRenewalResult.Failed(
					ConfirmSubscriptionRenewalFailure.SubscriptionPlanUnavailable);
			}

			var billingPeriodFrom = renewal.CurrentEndsAtSnapshot;

			var billingPeriodTo = billingPeriodFrom.AddMonths(
				plan.BillingCycleMonths);

			var now = timeProvider.GetUtcNow();

			var affectedRows = await dbcontext.SubscriptionRenewalRequests
				.Where(x =>
					x.Id == requestId &&
					x.TenantId == tenantId &&
					x.Status == SubscriptionRenewalRequestStatus.AwaitingTenantDecision &&
					x.CurrentEndsAtSnapshot == renewal.CurrentEndsAtSnapshot)
				.ExecuteUpdateAsync(
					setters => setters
						.SetProperty(
							x => x.SelectedPlanId,
							(Guid?)plan.Id)
						.SetProperty(
							x => x.PlanNameSnapshot,
							plan.Name)
						.SetProperty(
							x => x.BillingCycleMonthsSnapshot,
							(int?)plan.BillingCycleMonths)
						.SetProperty(
							x => x.PriceSnapshot,
							(decimal?)plan.Price)
						.SetProperty(
							x => x.CurrencySnapshot,
							"VND")
						.SetProperty(
							x => x.BillingPeriodFrom,
							(DateOnly?)billingPeriodFrom)
						.SetProperty(
							x => x.BillingPeriodTo,
							(DateOnly?)billingPeriodTo)
						.SetProperty(
							x => x.TenantRespondedByUserId,
							(Guid?)userId)
						.SetProperty(
							x => x.TenantRespondedAt,
							(DateTimeOffset?)now)
						.SetProperty(
							x => x.Status,
							SubscriptionRenewalRequestStatus.AwaitingPlatformReview)
						.SetProperty(
							x => x.UpdatedAt,
							(DateTimeOffset?)now),
						cancellationToken);

			if(affectedRows == 0)
			{
				var latestState = await dbcontext.SubscriptionRenewalRequests
					.AsNoTracking()
					.Where(x =>
						x.Id == requestId &&
						x.TenantId == tenantId)
					.Select(x => new ConfirmRenewalRow(
						x.Status,
						x.CurrentEndsAtSnapshot,
						x.Subscription.Status,
						x.Subscription.EndsAt))
					.SingleOrDefaultAsync(cancellationToken);

				if(latestState is null)
				{
					return ConfirmSubscriptionRenewalResult.Failed(
						ConfirmSubscriptionRenewalFailure.RenewalRequestNotFound);
				}

				if(latestState.Status != 
					SubscriptionRenewalRequestStatus.AwaitingTenantDecision)
				{
					return ConfirmSubscriptionRenewalResult.Failed(
						ConfirmSubscriptionRenewalFailure.RenewalRequestAlreadyResponded);
				}

				return ConfirmSubscriptionRenewalResult.Failed(
					ConfirmSubscriptionRenewalFailure.RenewalRequestStale);
			}

			var response = new ConfirmSubscriptionRenewalResponse(
				requestId,
				"awaiting_platform_review",
				plan.Id,
				plan.Name,
				plan.BillingCycleMonths,
				plan.Price,
				"VND",
				billingPeriodFrom,
				billingPeriodTo,
				now);

			return ConfirmSubscriptionRenewalResult.Success(response);
		}

		private static bool IsSupportedBillingCycle(
			int billingCycleMonths)
		{
			return billingCycleMonths is 6 or 12 or 24;
		}

		private sealed record ConfirmRenewalRow(
			SubscriptionRenewalRequestStatus Status,
			DateOnly CurrentEndsAtSnapshot,
			SubscriptionStatusEnum SubscriptionStatus,
			DateOnly SubscriptionEndsAt);

		private sealed record SelectedPlanNow(
			Guid Id,
			string Name,
			int BillingCycleMonths,
			decimal Price,
			bool IsAvailable,
			bool IsDeleted);

		private bool TryGetCurrentIdentity(
			out Guid userId,
			out Guid tenantId)
		{
			userId = Guid.Empty;

			tenantId = Guid.Empty;

			var principal = httpContextAccessor.HttpContext?.User;

			var userIdValue = principal?.FindFirstValue(AppClaimTypes.Subject);

			var tenantIdValue = principal?.FindFirstValue(AppClaimTypes.TenantId);

			return Guid.TryParse(userIdValue, out userId) &&
				Guid.TryParse(tenantIdValue, out tenantId);
		}

		private static JsonElement ParseFeatureLimits(string featureLimits)
		{
			using var document = JsonDocument.Parse(
				string.IsNullOrWhiteSpace(featureLimits)
					? "{}"
					: featureLimits);

			return document.RootElement.Clone();
		}

		private sealed record RenewalRow(
			Guid RequestId,
			DateTimeOffset CreatedAt,
			DateOnly CurrentEndsAtSnapshot,
			Guid SubscriptionId,
			SubscriptionStatusEnum SubscriptionStatus,
			Guid CurrentPlanId,
			string CurrentPlanName,
			int CurrentBillingCycleMonths,
			DateOnly SubscriptionStartsAt,
			DateOnly SubscriptionEndsAt);

		private sealed record PlanRow(
			Guid Id,
			string Name,
			string? Description,
			int BillingCycleMonths,
			decimal Price,
			string FeatureLimits);

		private Task<bool> IsTenantAdminAsync(
			Guid userId,
			Guid tenantId,
			CancellationToken cancellationToken)
		{
			return dbcontext.UserRoles
				.AsNoTracking()
				.AnyAsync(x =>
					x.UserId == userId &&
					x.Scope == RoleScopeEnum.Tenant &&
					x.TenantId == tenantId &&
					x.RevokedAt == null &&

					x.Role.Scope == RoleScopeEnum.Tenant &&
					x.Role.TenantId == tenantId &&
					x.Role.Code == DbInitializer.TenantAdminRoleCode &&
					!x.Role.IsDeleted &&

					x.User.TenantId == tenantId &&
					!x.User.IsDeleted &&
					x.User.Status == UserStatusEnum.Active &&
					x.User.EmailConfirmed,
				cancellationToken);
		}
	}
}