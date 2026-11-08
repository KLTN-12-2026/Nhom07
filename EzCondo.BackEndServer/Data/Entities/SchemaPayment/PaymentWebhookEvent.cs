using System.Text.Json;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaPayment
{
	public class PaymentWebhookEvent
	{
		public Guid Id { get; set; }
		public Guid ProviderAccountId { get; set; }
		public Guid? PaymentRequestId { get; set; }
		public string? EventKey { get; set; }
		public int? OrderCode { get; set; }
		public string? ProviderPaymentLinkId { get; set; }
		public string? ProviderReference { get; set; }
		public bool SignatureValid { get; set; }
		public string RawPayload { get; set; }
		public ProcessingStatusEnum ProcessingStatus { get; set; }
		public int processingAttempts { get; set; }
		public DateTimeOffset? LastAttemptAt { get; set; }
		public DateTimeOffset? NextAttemptAt { get; set; }
		public DateTimeOffset? ReceivedAt { get; set; }
		public DateTimeOffset? ProcessedAt { get; set; }
		public string? ProcessingError { get; set; }

		public ProviderAccount ProviderAccount { get; set; }
		public PaymentRequest PaymentRequest { get; set; }

		public PaymentProviderConfig PaymentProviderConfig { get; set; }
	}
}
