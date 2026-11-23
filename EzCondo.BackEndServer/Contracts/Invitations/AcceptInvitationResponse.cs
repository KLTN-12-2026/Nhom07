namespace EzCondo.BackEndServer.Contracts.Invitations
{
	public sealed record AcceptInvitationResponse
	(
		Guid UserId,
		Guid TenantId,
		string TenantSlug,
		string Email,
		DateTimeOffset AcceptedAt);
}
