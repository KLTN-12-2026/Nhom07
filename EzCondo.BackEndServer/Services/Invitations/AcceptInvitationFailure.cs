namespace EzCondo.BackEndServer.Services.Invitations
{
	public enum AcceptInvitationFailure
	{
		None,
		InvalidToken,
		InvitationNotSent,
		InvitationExpired,
		InvitationRevoked,
		InvitationAlreadyAccepted,
		TenantUnavailable,
		TargetUserUnavailable,
		RoleUnavailable,
		ActiveRoleAlreadyExists,
		PasswordRejected
	}
}
