using System.Text.Json;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class TenantPaymentWebhookEvent
	{
		public Guid Id { get; set; }
		public string? EventKey { get; set; }
		public int? OrderCode { get; set; }
		public bool IsSignatureValid { get; set; }
		public string Payload { get; set; }
		public DateTimeOffset ReceivedAt { get; set; }
		public DateTimeOffset? ProcessedAt { get; set; }
		public string? ProcessingError { get; set; }

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }
		public Guid ProviderConfigId { get; set; }
		public TenantPaymentProviderConfig PaymentProviderConfig { get; set; }
	}
}
