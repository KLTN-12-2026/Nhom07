using EzCondo.BackEndServer.Contracts.Invitations;

namespace EzCondo.BackEndServer.Services.Invitations
{
	public interface IInvitationAcceptanceService
	{
		Task<AcceptInvitationResult> AcceptAsync(
			AcceptInvitationRequest request,
			CancellationToken cancellationToken = default);
	}
}
