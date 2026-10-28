using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class SubscriptionPaymentOrder : IAuditableEntity
	{
		public Guid Id { get; set; }
		public long OrderCode { get; set; }
		public string? ProviderPaymentLinkId { get; set; }
		public PaymentOrderStatus Status { get; set; }
		public Decimal Amount { get; set; }
		public Decimal AmountPaid { get; set; }
		public string Currency { get; set; }
		public string? CheckoutUrl { get; set; }
		public string? QrCode { get; set; }
		public string? ReturnUrl { get; set; }
		public string CancelUrl { get; set; }
		public DateTimeOffset ExpiresAt { get; set; }
		public DateTimeOffset CancelledAt { get; set; }
		public string? CancellationReason { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public Guid SubscriptionInvoiceId { get; set; }
		public SubscriptionInvoice SubscriptionInvoice { get; set; } = null!;
		public Guid ProviderConfigId { get; set; }
		public PlatformPaymentProviderConfig ProviderConfig { get; set; }
		public Guid? RequestedBy { get; set; }
		public IdentityUser RequestedByUser { get; set; }

		public ICollection<SubscriptionPaymentTransaction> PaymentTransactions { get; set; }
		= new List<SubscriptionPaymentTransaction>();
		public ICollection<SubscriptionPaymentWebhookEvent> WebhookEvents { get; set; }
			= new List<SubscriptionPaymentWebhookEvent>();
	}
}
