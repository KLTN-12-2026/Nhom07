namespace EzCondo.BackEndServer.Infrastructure.Payments.PayOS.Models
{
	public sealed record CreatePayOSPaymentLinkCommand
	(
		long OrderCode,
		long Amount,
		string Description,
		DateTimeOffset ExpiresAt);
}
