namespace EzCondo.BackEndServer.Services.SubscriptionInvoices
{
	public enum CreateSubscriptionPaymentLinkFailure
	{
		None,
		InvalidIdentity,
		TenantAdminRequired,
		InvoiceNotFound,
		InvoiceNotPayable,
		RenewalRequestNotReady,
		ProviderAccountUnavailable,
		PaymentLinkCreationInProgress,
		PaymentReconciliationPending,
		PaymentProviderUnavailable
	}
}
