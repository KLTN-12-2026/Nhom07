namespace EzCondo.BackEndServer.Infrastructure.Invitations
{
	public interface IInvitationTokenService
	{
		InvitationToken Generate();

		string ComputeHash(string token);
	}
}
