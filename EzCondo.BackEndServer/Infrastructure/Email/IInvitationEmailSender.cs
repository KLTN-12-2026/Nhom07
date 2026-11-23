namespace EzCondo.BackEndServer.Infrastructure.Email
{
	public interface IInvitationEmailSender
	{
		Task SendAsync(
			InvitationEmailMessage email,
			CancellationToken cancellationToken = default);
	}
}
