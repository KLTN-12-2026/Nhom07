namespace EzCondo.BackEndServer.Services.SubscriptionInvoices
{
	public enum IssueSubscriptionInvoiceFailure
	{
		None,
		InvalidIdentity,
		PlatformAdminRequired,
		InvoiceNotFound,
		InvoiceNotDraft,
		RenewalRequestNotFound,
		RenewalRequestNotReady,
		RenewalRequestStale,
		InvoiceSnapshotMismatch,
		InvoiceDueDateElapsed
	}
}
