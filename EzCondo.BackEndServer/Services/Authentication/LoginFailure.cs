namespace EzCondo.BackEndServer.Services.Authentication
{
	public enum LoginFailure
	{
		None,
		InvalidCredentials,
		LockedOut,
		AccountUnavailable,
		TenantUnavailable,
		NoActiveRole
	}
}
