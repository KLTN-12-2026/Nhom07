namespace EzCondo.BackEndServer.Services.SubscriptionRenewals
{
	public sealed record SubscriptionRenewalReminderRunResult
	(
		int ScannedCount,
		int EligibleCount,
		int CreatedCount,
		int AlreadyExistsCount,
		int SkippedStateCount,
		int MissingTenantAdminCount
	);
}
