namespace EzCondo.BackEndServer.Services.SubscriptionRenewals
{
	public enum DeclineSubscriptionRenewalFailure
	{
		None,
		InvalidIdentity,
		TenantAdminRequired,
		RenewalRequestNotFound,
		RenewalRequestAlreadyResponded,
		RenewalRequestStale
	}
}
