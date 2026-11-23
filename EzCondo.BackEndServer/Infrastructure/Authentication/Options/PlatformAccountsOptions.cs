namespace EzCondo.BackEndServer.Infrastructure.Authentication.Options
{
	public sealed class PlatformAccountsOptions
	{
		public const string SectionName = "PlatformAccounts";

		public Dictionary<string, PlatformAccountConfig> Accounts { get; set; }
			= new(StringComparer.OrdinalIgnoreCase);
	}

	public sealed class PlatformAccountConfig
	{
		public required string Email { get; set; }
		public required string FullName { get; set; }
		public required string CitizenId { get; set; }
		public required string Password { get; set; }
	}
}
