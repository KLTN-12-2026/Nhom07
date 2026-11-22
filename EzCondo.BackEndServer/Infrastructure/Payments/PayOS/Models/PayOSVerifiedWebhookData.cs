namespace EzCondo.BackEndServer.Infrastructure.Payments.PayOS.Models
{
	public sealed record PayOSVerifiedWebhookData
	(
		long OrderCode,
		long Amount,
		string Description,
		string AccountNumber,
		string Reference,
		string TransactionDateTime,
		string Currency,
		string PaymentLinkId,
		string Code,
		string ProviderDescription,
		string CounterAccountBankId,
		string CounterAccountBankName,
		string CounterAccountName,
		string CounterAccountNumber,
		string VirtualAccountName,
		string VirtualAccountNumber);
}
