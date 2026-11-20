namespace EzCondo.BackEndServer.Contracts.SubscriptionInvoices
{
	public sealed record ActionRequiredSubscriptionInvoiceItemResponse
	(
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
		string Currency,
		bool IsOverdue,
		bool CanPay)
	{
		public decimal RemainingAmount =>
			TotalAmount - PaidAmount;
	}
}
