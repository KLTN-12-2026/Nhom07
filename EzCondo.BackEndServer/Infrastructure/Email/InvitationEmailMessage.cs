namespace EzCondo.BackEndServer.Infrastructure.Email
{
	public sealed record InvitationEmailMessage
	(
		string RecipientEmail,
		string RecipientName,
		string TenantName,
		string TenantSlug,
		InvitationRecipientType RecipientType,
		string RawToken,
		DateTimeOffset TokenExpiresAt
		);
}
