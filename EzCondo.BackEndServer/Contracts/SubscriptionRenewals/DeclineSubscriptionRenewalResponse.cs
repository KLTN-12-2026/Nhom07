namespace EzCondo.BackEndServer.Contracts.SubscriptionRenewals
{
	public sealed record DeclineSubscriptionRenewalResponse
	(
		Guid RequestId,
		string Status,
		DateOnly SubscriptionEndsAt,
		DateTimeOffset AccessExpiresAt,
		DateTimeOffset TenantRespondedAt);
}
