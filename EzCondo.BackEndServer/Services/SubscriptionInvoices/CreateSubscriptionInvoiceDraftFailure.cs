namespace EzCondo.BackEndServer.Services.SubscriptionInvoices
{
	public enum CreateSubscriptionInvoiceDraftFailure
	{
		None,
		InvalidIdentity,
		PlatformAdminRequired,
		RenewalRequestNotFound,
		RenewalRequestAlreadyReviewed,
		RenewalRequestStale,
		RenewalSnapshotInvalid,
		InvoiceAlreadyExistsForPeriod
	}
}
