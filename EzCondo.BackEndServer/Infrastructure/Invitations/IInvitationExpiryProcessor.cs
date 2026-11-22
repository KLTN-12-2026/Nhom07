namespace EzCondo.BackEndServer.Infrastructure.Invitations
{
	public interface IInvitationExpiryProcessor
	{
		Task<int> ExpireAsync(
			CancellationToken cancellationToken = default);
	}
}
