namespace EzCondo.BackEndServer.Services.SubscriptionRenewals
{
	public sealed record SubscriptionLifecycleRunResult
	(
		int ScannedCount,
		int NoticeSentCount,
		int NoticeFailedCount,
		int ExpiredCount);
}
