namespace EzCondo.BackEndServer.Data.Entities
{
	public class SubscriptionPaymentWebhookEvent
	{
		public Guid Id { get; set; }
		public string? EventKey { get; set; }
		public long? OrderCode { get; set; }
		public bool IsSignatureValid { get; set; }
		public string Payload { get; set; } = null!;
		public DateTimeOffset ReceivedAt { get; set; }
		public DateTimeOffset? ProcessedAt { get; set; }
		public int ProcessingAttempts { get; set; }
		public DateTimeOffset? LastAttemptAt { get; set; }
		public string? ProcessingError { get; set; }

		public Guid ProviderConfigId { get; set; }
		public PlatformPaymentProviderConfig ProviderConfig { get; set; } = null!;
		public Guid? PaymentOrderId { get; set; }
		public SubscriptionPaymentOrder? PaymentOrder { get; set; }

	}
}
