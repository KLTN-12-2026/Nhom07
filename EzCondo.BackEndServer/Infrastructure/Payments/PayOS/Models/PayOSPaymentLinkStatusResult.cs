namespace EzCondo.BackEndServer.Infrastructure.Payments.PayOS.Models
{
	public sealed record PayOSPaymentLinkStatusResult
	(
		long OrderCode,
		string PaymentLinkId,
		long Amount,
		long AmountPaid,
		long AmountRemaining,
		string ProviderStatus
	);
}
