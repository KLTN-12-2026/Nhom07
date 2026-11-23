namespace EzCondo.BackEndServer.Services.SubscriptionRenewals
{
	public enum GetCurrentSubscriptionRenewalFailure
	{
		None,
		InvalidIdentity,
		TenantAdminRequired,
		RenewalRequestNotFound,
		RenewalRequestStale
	}
}
