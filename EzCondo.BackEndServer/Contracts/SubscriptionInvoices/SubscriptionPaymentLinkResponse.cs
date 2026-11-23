namespace EzCondo.BackEndServer.Contracts.SubscriptionInvoices
{
	public sealed record SubscriptionPaymentLinkResponse(
		Guid InvoiceId,
		string InvoiceNo,
		Guid PaymentRequestId,
		Guid PaymentAttemptId,
		long OrderCode,
		string ProviderPaymentLinkId,
		decimal Amount,
		string Currency,
		string Status,
		string CheckoutUrl,
		string QrCode,
		DateTimeOffset ExpiresAt,
		bool Reused);
}