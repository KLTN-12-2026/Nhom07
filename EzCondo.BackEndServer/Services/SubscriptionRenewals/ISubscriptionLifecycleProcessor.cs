namespace EzCondo.BackEndServer.Services.SubscriptionRenewals
{
	public interface ISubscriptionLifecycleProcessor
	{
		Task<SubscriptionLifecycleRunResult> ProcessAsync(
			CancellationToken cancellationToken = default);
	}
}
