namespace EzCondo.BackEndServer.Infrastructure.Subscriptions
{
	public interface ITenantBackgroundJobCoordinator
	{
		CancellationToken GetToken(Guid tenantId);

		bool IsStopped(Guid tenantId);

		void Stop(Guid tenantId);

		void Resume(Guid tenantId);
	}
}
