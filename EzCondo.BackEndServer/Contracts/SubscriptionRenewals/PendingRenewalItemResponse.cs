namespace EzCondo.BackEndServer.Contracts.SubscriptionRenewals
{
	public sealed record PendingRenewalItemResponse
	(
		Guid RequestId,
		string Status,
		PendingRenewalTenantResponse Tenant,
		Guid SubscriptionId,
		DateOnly CurrentEndsAt,
		PendingRenewalPlanResponse SelectedPlan,
		DateOnly BillingPeriodFrom,
		DateOnly BillingPeriodTo,
		PendingRenewalResponderResponse TenantRespondedBy,
		DateTimeOffset TenantRespondedAt);
}
