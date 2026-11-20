namespace EzCondo.BackEndServer.Contracts.SubscriptionRenewals
{
	public sealed record CurrentSubscriptionRenewalResponse
	(
		Guid RequestId,
		string Status,
		DateTimeOffset CreatedAt,
		CurrentSubscriptionSummaryResponse CurrentSubscription,
		DateOnly NextBillingPeriodStartsAt,
		IReadOnlyCollection<SubscriptionRenewalPlanOptionResponse> AvailablePlans);
}
