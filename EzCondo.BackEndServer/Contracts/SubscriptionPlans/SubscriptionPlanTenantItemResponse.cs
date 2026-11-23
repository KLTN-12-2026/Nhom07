using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Contracts.SubscriptionPlans
{
	public sealed record SubscriptionPlanTenantItemResponse
	(
		Guid TenantId,
		string TenantCode,
		string TenantSlug,
		string LegalName,
		TenantStatusEnum TenantStatus,
		Guid SubscriptionId,
		SubscriptionStatusEnum SubscriptionStatus,
		int BillingCycleMonthsSnapshot,
		DateOnly StartsAt,
		DateOnly EndsAt);
}
