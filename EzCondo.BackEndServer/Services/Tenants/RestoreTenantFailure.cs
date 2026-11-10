namespace EzCondo.BackEndServer.Services.Tenants
{
	public enum RestoreTenantFailure
	{
		None,
		TenantNotFound,
		TenantNotSuspended,
		NoValidSubscription
	}
}
