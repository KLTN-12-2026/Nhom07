using EzCondo.BackEndServer.Contracts.SubscriptionInvoices;
using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EzCondo.BackEndServer.Services.SubscriptionInvoices
{
	public sealed class TenantSubscriptionInvoiceService(
		ApplicationDbContext dbcontext,
		IHttpContextAccessor httpContextAccessor,
		TimeProvider timeProvider)
		: ITenantSubscriptionInvoiceService
	{
		public async Task<GetActionRequiredSubscriptionInvoiceResult> GetActionRequiredAsync(
			CancellationToken cancellationToken = default)
		{
			if(!TryGetCurrentIdentity(
				out var userId,
				out var tenantId))
			{
				return GetActionRequiredSubscriptionInvoiceResult.Failed(
					GetActionRequiredSubscriptionInvoicesFailure.InvalidIdentity);
			}

			var isTenantAdmin = await IsTenantAdminAsync(
				userId,
				tenantId,
				cancellationToken);

			if (!isTenantAdmin)
			{
				return GetActionRequiredSubscriptionInvoiceResult.Failed(
					GetActionRequiredSubscriptionInvoicesFailure.TenantAdminRequired);
			}

			var now = timeProvider.GetUtcNow();

			var rows = await dbcontext.SubscriptionRenewalRequests
				.AsNoTracking()
				.Where(
					x =>
						x.TenantId == tenantId &&
						x.Status == SubscriptionRenewalRequestStatus.InvoiceIssued &&

						x.SubscriptionInvoiceId != null &&
						x.SelectedPlanId != null &&
						x.PlanNameSnapshot != null &&
						x.BillingPeriodFrom != null &&
						x.BillingPeriodTo != null &&

						x.SubscriptionInvoice != null &&
						x.SubscriptionInvoice.TenantId == tenantId &&
						!x.SubscriptionInvoice.IsDeleted &&
						x.SubscriptionInvoice.Status == SubscriptionInvoiceStatus.Issued &&
						x.SubscriptionInvoice.IssuedAt != null &&
						x.SubscriptionInvoice.PaidAmount < x.SubscriptionInvoice.TotalAmount)
				.OrderBy(x => x.SubscriptionInvoice!.DueAt)
				.ThenBy(x => x.SubscriptionInvoice!.InvoiceNo)
				.Select(x => new ActionRequiredInvoiceRow(
					x.SubscriptionInvoice!.Id,
					x.SubscriptionInvoice.InvoiceNo,
					x.Id,
					x.SubscriptionId,
					x.SelectedPlanId!.Value,
					x.PlanNameSnapshot!,
					x.BillingPeriodFrom!.Value,
					x.BillingPeriodTo!.Value,
					x.SubscriptionInvoice.IssuedAt!.Value,
					x.SubscriptionInvoice.DueAt,
					x.SubscriptionInvoice.TotalAmount,
					x.SubscriptionInvoice.PaidAmount,
					x.SubscriptionInvoice.Currency))
				.ToListAsync(cancellationToken);

			var items = rows.Select(
				x =>
					new ActionRequiredSubscriptionInvoiceItemResponse(
						x.InvoiceId,
						x.InvoiceNo,
						x.RenewalRequestId,
						x.SubscriptionId,
						x.SubscriptionPlanId,
						x.SubscriptionPlanName,
						x.BillingPeriodFrom,
						x.BillingPeriodTo,
						x.IssuedAt,
						x.DueAt,
						x.TotalAmount,
						x.PaidAmount,
						x.Currency.Trim(),
						x.DueAt <= now,
						true))
				.ToArray();

			var response = new ActionRequiredSubscriptionInvoicesResponse(
				items.Length,
				items);

			return GetActionRequiredSubscriptionInvoiceResult.Success(response);
		}

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

		private Task<bool> IsTenantAdminAsync(
			Guid userId,
			Guid tenantId,
			CancellationToken cancellationToken)
		{
			return dbcontext.UserRoles
				.AsNoTracking()
				.AnyAsync(
					x =>
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

		private sealed record ActionRequiredInvoiceRow(
			Guid InvoiceId,
			string InvoiceNo,
			Guid RenewalRequestId,
			Guid SubscriptionId,
			Guid SubscriptionPlanId,
			string SubscriptionPlanName,
			DateOnly BillingPeriodFrom,
			DateOnly BillingPeriodTo,
			DateTimeOffset IssuedAt,
			DateTimeOffset DueAt,
			decimal TotalAmount,
			decimal PaidAmount,
			string Currency);
	}
}
