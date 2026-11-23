using EzCondo.BackEndServer.Contracts.SubscriptionRenewals;
using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EzCondo.BackEndServer.Services.SubscriptionRenewals
{
	public sealed class PlatformSubscriptionRenewalService(
		ApplicationDbContext dbcontext,
		IHttpContextAccessor httpContextAccessor)
		: IPlatformSubscriptionRenewalService
	{
		public async Task<GetPendingRenewalsResult> GetPendingReviewAsync(
			GetPendingRenewalsRequest request, 
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(request);

			if(request.PageNumber < 1 ||
				request.PageSize is < 1 or > 100)
			{
				return GetPendingRenewalsResult.Failed(
					GetPendingRenewalsFailure.InvalidPagination);
			}

			if(!TryGetCurrentUserId(out var platformAdminId))
			{
				return GetPendingRenewalsResult.Failed(
					GetPendingRenewalsFailure.InvalidIdentity);
			}

			var isPlatformAdmin = await IsPlatformAdminAsync(
				platformAdminId,
				cancellationToken);

			if (!isPlatformAdmin)
			{
				return GetPendingRenewalsResult.Failed(
					GetPendingRenewalsFailure
						.PlatformAdminRequired);
			}

			var query = dbcontext.SubscriptionRenewalRequests
				.AsNoTracking()
				.Where(x =>
					x.Status == SubscriptionRenewalRequestStatus.AwaitingPlatformReview &&
					x.SubscriptionInvoiceId == null &&
					x.PlatformReviewedByAdminId == null &&
					x.PlatformReviewedAt == null);

			var totalCount = await query.CountAsync(cancellationToken);

			var totalPages = totalCount == 0
				? 0
				: (int)Math.Ceiling(
					totalCount /
					(double)request.PageSize);

			IReadOnlyCollection<PendingRenewalItemResponse> items;

			if(totalCount == 0 ||
				request.PageNumber > totalPages)
			{
				items = Array.Empty<PendingRenewalItemResponse>();
			}
			else
			{
				var skip = checked(
					(request.PageNumber - 1) *
					request.PageSize);

				var rows = await query
					.OrderBy(x => x.TenantRespondedAt)
					.ThenBy(x => x.Id)
					.Skip(skip)
					.Take(request.PageSize)
					.Select(x => new PendingRenewalRow(
						x.Id,
						x.TenantId,
						x.Tenant.Code,
						x.Tenant.Slug,
						x.Tenant.LegalName,
						x.SubscriptionId,
						x.CurrentEndsAtSnapshot,

						x.SelectedPlanId!.Value,
						x.PlanNameSnapshot!,
						x.BillingCycleMonthsSnapshot!.Value,
						x.PriceSnapshot!.Value,
						x.CurrencySnapshot!,

						x.BillingPeriodFrom!.Value,
						x.BillingPeriodTo!.Value,

						x.TenantRespondedByUserId!.Value,
						x.TenantRespondedByUser!.FullName,
						x.TenantRespondedByUser.Email!,
						x.TenantRespondedAt!.Value))
					.ToListAsync(cancellationToken);

				items = rows
					.Select(ToResponse)
					.ToArray();
			}

			return GetPendingRenewalsResult.Success(
				new PendingRenewalsResponse(
					request.PageNumber,
					request.PageSize,
					totalCount,
					totalPages,
					items));
		}

		private bool TryGetCurrentUserId(out Guid userId)
		{
			userId = Guid.Empty;

			var value = httpContextAccessor
				.HttpContext?
				.User
				.FindFirstValue(AppClaimTypes.Subject);

			return Guid.TryParse(value, out userId);
		}

		private Task<bool> IsPlatformAdminAsync(
			Guid userId,
			CancellationToken cancellationToken)
		{
			return dbcontext.UserRoles
				.AsNoTracking()
				.AnyAsync(x =>
					x.UserId == userId &&
					x.Scope == RoleScopeEnum.Platform &&
					x.TenantId == null &&
					x.RevokedAt == null &&

					x.Role.Scope == RoleScopeEnum.Platform &&
					x.Role.TenantId == null &&
					x.Role.Code == DbInitializer.PlatformAdminRoleCode &&
					!x.Role.IsDeleted &&

					x.User.TenantId == null &&
					!x.User.IsDeleted &&
					x.User.Status == UserStatusEnum.Active &&
					x.User.EmailConfirmed,
				cancellationToken);
		}

		private static PendingRenewalItemResponse ToResponse(
			PendingRenewalRow row)
		{
			return new PendingRenewalItemResponse(
				row.RequestId,
				"awaiting_platform_review",
				new PendingRenewalTenantResponse(
					row.TenantId,
					row.TenantCode,
					row.TenantSlug,
					row.TenantLegalName),
				row.SubscriptionId,
				row.CurrentEndsAt,
				new PendingRenewalPlanResponse(
					row.SelectedPlanId,
					row.PlanName,
					row.BillingCycleMonths,
					row.Price,
					row.Currency),
				row.BillingPeriodFrom,
				row.BillingPeriodTo,
				new PendingRenewalResponderResponse(
					row.TenantRespondedByUserId,
					row.TenantRespondedByFullName,
					row.TenantRespondedByEmail),
				row.TenantRespondedAt);
		}

		private sealed record PendingRenewalRow(
			Guid RequestId,
			Guid TenantId,
			string TenantCode,
			string TenantSlug,
			string TenantLegalName,
			Guid SubscriptionId,
			DateOnly CurrentEndsAt,
			Guid SelectedPlanId,
			string PlanName,
			int BillingCycleMonths,
			decimal Price,
			string Currency,
			DateOnly BillingPeriodFrom,
			DateOnly BillingPeriodTo,
			Guid TenantRespondedByUserId,
			string TenantRespondedByFullName,
			string TenantRespondedByEmail,
			DateTimeOffset TenantRespondedAt);
	}
}
