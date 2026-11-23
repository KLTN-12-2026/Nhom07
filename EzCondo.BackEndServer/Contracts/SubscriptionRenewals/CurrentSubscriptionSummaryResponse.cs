namespace EzCondo.BackEndServer.Contracts.SubscriptionRenewals
{
	public sealed record CurrentSubscriptionSummaryResponse//info sub hiện tại
	(
		Guid SubscriptionId,
		Guid CurrentPlanId,
		string CurrentPlanName,
		int CurrentBillingCycleMonths,
		DateOnly StartsAt,
		DateOnly EndsAt);
}
