namespace EzCondo.BackEndServer.Services.SubscriptionInvoices
{
	public interface ITenantSubscriptionInvoiceService
	{
		Task<GetActionRequiredSubscriptionInvoiceResult> GetActionRequiredAsync(
			CancellationToken cancellationToken = default);
	}
}
