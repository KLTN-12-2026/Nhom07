namespace EzCondo.BackEndServer.Services.SubscriptionRenewals
{
	public interface ISubscriptionRenewalReminderProcessor
	{
		Task<SubscriptionRenewalReminderRunResult> ProcessAsync(
			CancellationToken cancellationToken = default);
	}
}
