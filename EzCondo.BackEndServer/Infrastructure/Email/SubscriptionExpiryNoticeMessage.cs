namespace EzCondo.BackEndServer.Infrastructure.Email
{
	public sealed record SubscriptionExpiryNoticeMessage
	(
		string RecipientEmail,
		string RecipientName,
		string TenantName,
		string PlanName,
		DateOnly SubscriptionEndsAt,
		DateTimeOffset AccessExpiresAt);
}
