using EzCondo.BackEndServer.Contracts.SubscriptionPlans;

namespace EzCondo.BackEndServer.Services.SubscriptionPlans
{
	public interface ISubscriptionPlanService
	{
		Task<SubscriptionPlanResponse> CreateAsync(
			CreateSubscriptionPlanRequest request,
			CancellationToken cancellationToken = default);

		Task<SubscriptionPlanResponse?> UpdateAsync(
			Guid id,
			UpdateSubscriptionPlanRequest request,
			CancellationToken cancellationToken = default);

		Task<IReadOnlyList<SubscriptionPlanResponse>> GetAsync(
			CancellationToken cancellationToken = default);

		Task<bool> DeleteAsync(
			Guid id,
			CancellationToken cancellationToken = default);
	}
}
