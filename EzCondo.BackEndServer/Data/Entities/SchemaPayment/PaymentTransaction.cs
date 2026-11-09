using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using EzCondo.BackEndServer.Data.Enums;
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
		public string ProviderTransactionId { get; set; } = null!;
		public Decimal Amount { get; set; }//
		public DateTimeOffset OccurredAt { get; set; }//
		public string PayerMasked { get; set; } = null!;
		public string ProviderPayload { get; set; } = null!;
		public DateTimeOffset CreatedAt { get; set; }//
		public string Currency { get; set; } = null!;
		public PaymentTransactionStatus Status { get; set; } = PaymentTransactionStatus.Received;

		public ProviderAccount ProviderAccount { get; set; } = null!;
		public PaymentRequest? PaymentRequests { get; set; }
		public PaymentWebhookEvent? SourceWebhookEvent { get; set; }

		public ICollection<SubscriptionPaymentAllocation> SubscriptionPaymentAllocations { get; set; } = new List<SubscriptionPaymentAllocation>();
		public ICollection<InvoicePaymentAllocation> InvoicePaymentAllocations { get; set; } = new List<InvoicePaymentAllocation>();

	}
}
