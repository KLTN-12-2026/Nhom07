namespace EzCondo.BackEndServer.Contracts.SubscriptionPlans
{
	public sealed record SubscriptionPlanTenantsResponse
	(
		Guid PlanId,
		string PlanName,
		bool IsAvailable,
		int PageNumber,
		int PageSize,
		int TotalCount,
		int TotalPages,
		IReadOnlyCollection<SubscriptionPlanTenantItemResponse> Items);
}
