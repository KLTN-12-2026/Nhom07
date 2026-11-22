namespace EzCondo.BackEndServer.Infrastructure.Payments.PayOS.Models
{
	public sealed record PayOSPaymentLinkResult
	(
		long OrderCode,
		long Amount,
		string Currency,
		string PaymentLinkId,
		string CheckoutUrl,
		string QrCode,
		string ProviderStatus,
		DateTimeOffset ExpiresAt);
}
