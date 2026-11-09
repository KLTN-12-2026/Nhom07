using EzCondo.BackEndServer.Data.Entities.SchemaPayment;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;

namespace EzCondo.BackEndServer.Data.Entities.SchemaTenant
{
	public sealed class InvoicePaymentAllocation
	{
		public Guid Id { get; set; }
		public Guid TenantId { get; set; }
		public Guid PaymentAttemptId { get; set; }
		public Guid GatewayAccountId { get; set; }
		public Guid GatewayTransactionId { get; set; }
		public decimal AmountApplied { get; set; }
		public DateTimeOffset CreatedAt { get; set; }

		public Tenant Tenant { get; set; } = null!;
		public InvoicePaymentAttempt InvoicePaymentAttempt { get; set; } = null!;
		public ProviderAccount ProviderAccount { get; set; } = null!;
		public PaymentTransaction PaymentTransaction { get; set; } = null!;
	}
}
