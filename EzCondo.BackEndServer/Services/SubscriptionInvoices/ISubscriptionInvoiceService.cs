namespace EzCondo.BackEndServer.Services.SubscriptionInvoices
{
	public interface ISubscriptionInvoiceService
	{
		Task<CreateSubscriptionInvoiceDraftResult> CreateDraftFromRenewalAsync(
			Guid renewalRequestId,
			CancellationToken cancellationToken = default);

		Task<IssueSubscriptionInvoiceResult> IssueAsync(
			Guid invoiceId,
			CancellationToken cancellationToken = default);
	}
}
