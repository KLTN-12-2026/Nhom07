namespace EzCondo.BackEndServer.Infrastructure.Email.Options
{
	public sealed class SmtpOptions
	{
		public const string SectionName = "Email:Smtp";
		public string Host { get; set; } = string.Empty;
		public int Port { get; set; } = 587;
		public bool UseStartTls { get; set; } = true;
		public string Username { get; set; } = string.Empty;
		public string AppPassword { get; set; } = string.Empty;
		public string FromAddress { get; set; } = string.Empty;
		public string FromName { get; set; } = "EzCondo";
	}
}
