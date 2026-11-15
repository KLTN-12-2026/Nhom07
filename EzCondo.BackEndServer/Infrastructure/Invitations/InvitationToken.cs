namespace EzCondo.BackEndServer.Infrastructure.Invitations
{
	public sealed record InvitationToken
	(
		string Value,
		string Hash);
}
