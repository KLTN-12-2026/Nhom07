using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Contracts.SubscriptionInvoices
{
	public sealed record SubscriptionInvoiceResponse
	(
		Guid Id,
		Guid TenantId,
		string TenantCode,
		string TenantSlug,
		string TenantLegalName,
		Guid SubscriptionId,
		Guid SubscriptionPlanId,
		string SubscriptionPlanName,
		string InvoiceNo,
		DateOnly BillingPeriodFrom,
		DateOnly BillingPeriodTo,
		DateTimeOffset DueAt,
		SubscriptionInvoiceStatus Status,
		decimal TotalAmount,
		decimal PaidAmount,
		string Currency,
		Guid CreatedByAdminId,
		DateTimeOffset CreatedAt
	);
}