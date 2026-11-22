namespace EzCondo.BackEndServer.Services.SubscriptionInvoices
{
	public interface ITenantSubscriptionPaymentLinkService
	{
		Task<CreateSubscriptionPaymentLinkResult> CreateAsync(
			Guid invoiceId,
			CancellationToken cancellationToken = default);
	}
}
