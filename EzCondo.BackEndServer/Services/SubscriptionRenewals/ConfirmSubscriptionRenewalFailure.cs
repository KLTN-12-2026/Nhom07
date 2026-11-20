namespace EzCondo.BackEndServer.Services.SubscriptionRenewals
{
	public enum ConfirmSubscriptionRenewalFailure
	{
		None,
		InvalidIdentity,
		TenantAdminRequired,
		RenewalRequestNotFound,
		RenewalRequestAlreadyResponded,
		RenewalRequestStale,
		SubscriptionPlanNotFound,
		SubscriptionPlanUnavailable
	}
}
