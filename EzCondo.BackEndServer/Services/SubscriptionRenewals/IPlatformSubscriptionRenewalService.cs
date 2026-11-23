using EzCondo.BackEndServer.Contracts.SubscriptionRenewals;

namespace EzCondo.BackEndServer.Services.SubscriptionRenewals
{
	public interface IPlatformSubscriptionRenewalService
	{
		Task<GetPendingRenewalsResult> GetPendingReviewAsync(
			GetPendingRenewalsRequest request,
			CancellationToken cancellationToken = default);
	}
}
