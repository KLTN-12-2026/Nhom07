namespace EzCondo.BackEndServer.Infrastructure.Invitations.Options
{
	public sealed class InvitationLinkOptions
	{
		public const string SectionName = "Invitations";
		public string PortalBaseUrl { get; set; } = string.Empty;
		public string AcceptPath { get; set; } = "/accept-invitation";
	}
}
