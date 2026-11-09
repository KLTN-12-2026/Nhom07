using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaPayment
{
	public class PaymentRequest : IAuditableEntity //Link/Qr thu tiền
	{
		public Guid Id { get; set; }
		public Guid ProviderAccountId { get; set; }
		public long OrderCode { get; set; }
		public string? ProviderPaymentLinkId { get; set; }
		public PaymentRequestStatus Status { get; set; } = PaymentRequestStatus.Pending;
		public decimal Amount { get; set; }
		public decimal AmountReceived { get; set; }
		public string Currency { get; set; } = null!;
		public string? CheckoutUrl { get; set; }
		public string? QrCode { get; set; }
		public string? ReturnUrl { get; set; }
		public string? CancelUrl { get; set; }
		public DateTimeOffset? ExpiresAt { get; set; }
		public DateTimeOffset? CancelledAt { get; set; }
		public string? CancellationReason { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public ProviderAccount ProviderAccounts { get; set; } = null!;

		public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
		public ICollection<PaymentWebhookEvent> PaymentWebhookEvents { get; set; } = new List<PaymentWebhookEvent>();
		public ICollection<SubscriptionPaymentAttempt> SubscriptionPaymentAttempts { get; set; } = new List<SubscriptionPaymentAttempt>();
		public ICollection<InvoicePaymentAttempt> InvoicePaymentAttempts { get; set; } = new List<InvoicePaymentAttempt>();

	}
}
