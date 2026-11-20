using System.Text.Json;

namespace EzCondo.BackEndServer.Contracts.SubscriptionRenewals
{
	public sealed record SubscriptionRenewalPlanOptionResponse//inffo sub có thể lựa chọn
	(
		Guid PlanId,
		string Name,
		string? Description,
		int BillingCycleMonths,
		decimal Price,
		string Currency,
		JsonElement FeatureLimits);
}
