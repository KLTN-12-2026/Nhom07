namespace EzCondo.BackEndServer.Infrastructure.Authentication
{
	public static class AuthPolicies
	{
		public const string PlatformScope = "Scope:platform";
		public const string TenantScope = "Scope:tenant";
		public const string PlatformTenantInvitationPermission =
			"platform.tenant_invitations.create";

		public const string PlatformTenantInvitationCreate =
			"Permission:platform.tenant_invitations.create";
	}
}
