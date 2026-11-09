using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;

namespace EzCondo.BackEndServer.Data.Entities.SchemaPayment
{
	public sealed class SubscriptionPaymentAllocation
	{
		public Guid Id { get; set; }
		public Guid PaymentAttemptId { get; set; }
		public Guid GatewayAccountId { get; set; }
		public Guid GatewayTransactionId { get; set; }
		public decimal AmountApplied { get; set; }
		public DateTimeOffset CreatedAt { get; set; }

		public SubscriptionPaymentAttempt SubscriptionPaymentAttempt { get; set; } = null!;
		public ProviderAccount ProviderAccount { get; set; } = null!;
		public PaymentTransaction PaymentTransaction { get; set; } = null!;
	}
}
