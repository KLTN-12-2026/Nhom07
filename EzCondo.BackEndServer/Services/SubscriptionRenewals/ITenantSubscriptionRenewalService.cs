using EzCondo.BackEndServer.Contracts.SubscriptionRenewals;

namespace EzCondo.BackEndServer.Services.SubscriptionRenewals
{
	public interface ITenantSubscriptionRenewalService
	{
		Task<GetCurrentSubscriptionRenewalResult> GetCurrentAsync(
			CancellationToken cancellationToken = default);

		Task<ConfirmSubscriptionRenewalResult> ConfirmAsync(
			Guid requestId,
			ConfirmSubscriptionRenewalRequest request,
			CancellationToken cancellationToken = default);

		Task<DeclineSubscriptionRenewalResult> DeclineAsync(
			Guid requestId,
			CancellationToken cancellationToken = default);
	}
}
