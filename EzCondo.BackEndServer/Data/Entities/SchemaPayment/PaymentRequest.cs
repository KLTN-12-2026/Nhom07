using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaPayment
{
	public class PaymentRequest : IAuditableEntity //Link/Qr thu tiền
	{
		public Guid Id { get; set; }
		public Guid ProviderAccountId { get; set; }
		public int OrderCode { get; set; }
		public string? ProviderPaymentLinkId { get; set; }
		public PaymentRequestStatus Status { get; set; }
		public decimal Amount { get; set; }
		public decimal AmountRecived { get; set; }
		public string Currency { get; set; }
		public string? CheckoutUrl { get; set; }
		public string? QrCode { get; set; }
		public string? ReturnUrl { get; set; }
		public string? CancelUrl { get; set; }
		public DateTimeOffset? ExpiredAt { get; set; }
		public DateTimeOffset? CancelledAt { get; set; }
		public string CancellationReason { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public ProviderAccount ProviderAccounts { get; set; }

		public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
		public ICollection<PaymentWebhookEvent> PaymentWebhookEvents { get; set; } = new List<PaymentWebhookEvent>();

	}
}
