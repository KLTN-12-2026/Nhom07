namespace EzCondo.BackEndServer.Contracts.SubscriptionRenewals
{
	public sealed record PendingRenewalResponderResponse
	(
		Guid UserId,
		string FullName,
		string Email
	);
}
