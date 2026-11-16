namespace EzCondo.BackEndServer.Contracts.Authentication.Tenant
{
	public sealed record TenantLoginResponse
	(
		string AccessToken,
		string TokenType,
		DateTimeOffset ExpiresAt,
		Guid UserId,
		string Email,
		string FullName,
		Guid TenantId,
		string TenantSlug,
		IReadOnlyCollection<string> RoleCodes);
}
