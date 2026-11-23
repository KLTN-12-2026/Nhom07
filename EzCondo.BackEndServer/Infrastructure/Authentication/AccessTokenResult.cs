namespace EzCondo.BackEndServer.Infrastructure.Authentication
{
	public sealed record AccessTokenResult
	(
		string Token,
		DateTimeOffset ExpiresAt);
}
