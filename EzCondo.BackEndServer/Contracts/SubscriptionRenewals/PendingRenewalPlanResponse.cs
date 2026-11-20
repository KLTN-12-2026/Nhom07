namespace EzCondo.BackEndServer.Contracts.SubscriptionRenewals
{
	public sealed record PendingRenewalPlanResponse
	(
		Guid PlanId,
		string PlanName,
		int BillingCycleMonths,
		decimal Price,
		string Currency);
}
