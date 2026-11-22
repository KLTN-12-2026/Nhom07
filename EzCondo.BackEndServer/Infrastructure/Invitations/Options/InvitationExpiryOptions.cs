namespace EzCondo.BackEndServer.Infrastructure.Invitations.Options
{
	public sealed class InvitationExpiryOptions
	{
		public const string SectionName = "Invitations:Expiry";
		public bool Enabled { get; set; } = true;
		public int CheckIntervalMinutes { get; set; } = 10;
	}
}
