using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Contracts.SubscriptionInvoices
{
	public sealed record IssueSubscriptionInvoiceResponse
	(
		Guid InvoiceId,
		string InvoiceNo,
		
		Guid TenantId,
		string TenantCode,
		string TenantSlug,
		string TenantLegalName,
		
		Guid SubscriptionId,
		Guid RenewalRequestId,
		
		Guid SubscriptionPlanId,
		string SubscriptionPlanName,
		
		DateOnly BillingPeriodFrom,
		DateOnly BillingPeriodTo,
		DateTimeOffset DueAt,
		
		SubscriptionInvoiceStatus InvoiceStatus,
		SubscriptionRenewalRequestStatus RenewalStatus,
		
		decimal TotalAmount,
		decimal PaidAmount,
		string Currency,
		
		Guid IssuedByAdminId,
		DateTimeOffset IssuedAt)
	{
		public decimal RemainingAmount =>
			TotalAmount - PaidAmount;
	}
}
