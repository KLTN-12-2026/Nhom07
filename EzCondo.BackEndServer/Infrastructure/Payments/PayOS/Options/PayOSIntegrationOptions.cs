namespace EzCondo.BackEndServer.Infrastructure.Payments.PayOS.Options
{
	public sealed class PayOSIntegrationOptions
	{
		public const string SectionName = "PayOS";
		public string ClientId { get; set; } = string.Empty;
		public string ApiKey { get; set; } = string.Empty;
		public string ChecksumKey { get; set; } = string.Empty;
		public string ProviderAccountCode { get; set; } = string.Empty;
		public string ReturnUrl { get; set; } = string.Empty;
		public string CancelUrl { get; set; } = string.Empty;
		public int PaymentLinkMinutes { get; set; } = 30;
		public int TimeoutSeconds { get; set; } = 30;
		public int MaxRetries { get; set; } = 2;
	}
}
