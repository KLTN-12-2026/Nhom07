namespace EzCondo.BackEndServer.Services.Tenants
{
	public enum ScheduleTenantSuspensionFailure
	{
		None,
		TenantNotFound,
		TenantAlreadySuspended,
		TenantClosed,
		SuspensionAlreadyScheduled
	}
}
