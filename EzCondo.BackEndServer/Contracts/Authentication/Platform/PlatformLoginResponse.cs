namespace EzCondo.BackEndServer.Contracts.Authentication.Platform
{
	public sealed record PlatformLoginResponse
	(
		string AccessToken,
		string TokenType,
		DateTimeOffset ExpiresAt,
		Guid UserId,
		string Email,
		string FullName,
		IReadOnlyCollection<string> RoleCodes
	);
}
