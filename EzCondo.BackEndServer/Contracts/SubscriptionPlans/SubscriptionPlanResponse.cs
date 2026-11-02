using System.Text.Json;

namespace EzCondo.BackEndServer.Contracts.SubscriptionPlans
{
	public sealed record SubscriptionPlanResponse
	(
		Guid Id,
		string Name,
		string? Description,
		int BillingCycleMonths,
		decimal Price,
		JsonElement FeatureLimits,
		bool IsAvailable,
		DateTimeOffset CreatedAt,
		DateTimeOffset? UpdatedAt
	);
}
