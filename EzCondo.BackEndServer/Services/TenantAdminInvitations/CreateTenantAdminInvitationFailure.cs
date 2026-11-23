namespace EzCondo.BackEndServer.Services.TenantAdminInvitations
{
	public enum CreateTenantAdminInvitationFailure
	{
		None,
		TenantNotFound,
		TenantUnavailable,
		TenantAdminAlreadyExists,
		ActiveInvitationAlreadyExists,
		EmailAlreadyAssignedToAnotherAccount,
		TenantAdminRoleNotFound
	}
}
