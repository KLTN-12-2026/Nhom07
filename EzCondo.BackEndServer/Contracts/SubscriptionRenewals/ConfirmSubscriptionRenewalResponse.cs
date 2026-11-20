namespace EzCondo.BackEndServer.Contracts.SubscriptionRenewals
{
	public sealed record ConfirmSubscriptionRenewalResponse
	(
		Guid RequestId,
		string Status,
		Guid SelectedPlanId,
		string PlanName,
		int BillingCycleMonths,
		decimal Price,
		string Currency,
		DateOnly BillingPeriodFrom,
		DateOnly BillingPeriodTo,
		DateTimeOffset TenantRespondedAt);
}
