using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class SubscriptionPaymentTransaction
	{
		public Guid Id { get; set; }
		public string ProviderTransactionId { get; set; } = null!;
		public string? ProviderReference { get; set; }
		public PaymentTransactionStatus Status { get; set; }
		public Decimal Amount { get; set; }
		public DateTimeOffset OccurredAt { get; set; }
		public string PayerMasked { get; set; } = "{}";
		public string ProviderPayload { get; set; } = "{}";
		public DateTimeOffset CreatedAt { get; set; }

		public Guid ProviderConfigId { get; set; }
		public PlatformPaymentProviderConfig ProviderConfig { get; set; } = null!;
		public Guid PaymentOrderId { get; set; }
		public SubscriptionPaymentOrder PaymentOrder { get; set; }
	}
}
