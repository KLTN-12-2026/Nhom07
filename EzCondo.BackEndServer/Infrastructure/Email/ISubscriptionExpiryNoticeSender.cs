namespace EzCondo.BackEndServer.Infrastructure.Email
{
	public interface ISubscriptionExpiryNoticeSender
	{
		Task SendAsync(
			SubscriptionExpiryNoticeMessage message,
			CancellationToken cancellationToken = default);
	}
}
