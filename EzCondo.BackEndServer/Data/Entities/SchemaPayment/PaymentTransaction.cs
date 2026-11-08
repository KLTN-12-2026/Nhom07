using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using System.Text.Json;

namespace EzCondo.BackEndServer.Data.Entities.SchemaPayment
{
	public class PaymentTransaction //Giao dịch tiền vào
	{
		public Guid Id { get; set; }//
		public Guid ProviderAccountId { get; set; }
		public Guid? PaymentRequestId { get; set; }
		public Guid? SourceWebhookEventId { get; set; }
		public string? ProviderReference { get; set; }//
		public string ProviderTransactionId { get; set; }//
		public Decimal Amount { get; set; }//
		public DateTimeOffset OccurredAt { get; set; }//
		public string PayerMasked { get; set; }//
		public string ProviderPayload { get; set; }//
		public DateTimeOffset CreatedAt { get; set; }//
		public string Currency { get; set; }
		public PaymentTransactionStatus Status { get; set; }

		public ProviderAccount ProviderAccount { get; set; }
		public PaymentRequest PaymentRequests { get; set; }
	}
}
