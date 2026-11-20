namespace EzCondo.BackEndServer.Services.SubscriptionRenewals
{
	public enum GetPendingRenewalsFailure
	{
		None,
		InvalidIdentity,
		PlatformAdminRequired,
		InvalidPagination
	}
}
