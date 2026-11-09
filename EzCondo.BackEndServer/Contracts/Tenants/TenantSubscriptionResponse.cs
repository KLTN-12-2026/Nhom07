using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Contracts.Tenants
{
	public sealed record TenantSubscriptionResponse
	(
		Guid Id,
		Guid PlanId,
		string PlanName,
		SubscriptionStatusEnum Status,
		DateOnly StartsAt,
		DateOnly EndsAt
	);
}
