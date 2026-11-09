namespace EzCondo.BackEndServer.Infrastructure.Authentication.Options
{
	public sealed class PlatformAccountsOptions
	{
		public const string SectionName = "PlatformAccounts";

		public List<PlatformAccountConfig> Accounts { get; set; } = [];
	}

	public sealed class PlatformAccountConfig
	{
		public required string Email { get; set; }
		public required string FullName { get; set; }
		public required string CitizenId { get; set; }
		public required string Password { get; set; }
	}
}
