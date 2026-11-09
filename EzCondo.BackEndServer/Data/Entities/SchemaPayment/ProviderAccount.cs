using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaPayment
{
	public class ProviderAccount : IAuditableEntity //Kênh PAYOS
	{
		public Guid Id { get; set; }
		public Guid? TenantId { get; set; }
		public Guid WebhookKey { get; set; }
		public string Code { get; set; } = null!;
		public string Provider { get; set; } = null!;
		public ProviderOwnerScope OwnerScope { get; set; }
		public string? MerchantCode { get; set; }
		public string? PaymentCredentialsRef { get; set; }
		public string? PayoutCredentialsRef { get; set; }
		public bool SupportsCollection { get; set; }
		public bool SupportsPayout { get; set; }
		public string Settings { get; set; } = null!;
		public bool IsActive { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public Tenant? Tenant { get; set; }

		public ICollection<PaymentRequest> PaymentRequests { get; set; } = new List<PaymentRequest>();
		public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
		public ICollection<PaymentWebhookEvent> PaymentWebhookEvents { get; set; } = new List<PaymentWebhookEvent>();
		public ICollection<SubscriptionPaymentAttempt> SubscriptionPaymentAttempts { get; set; } = new List<SubscriptionPaymentAttempt>();
		public ICollection<SubscriptionPaymentAllocation> SubscriptionPaymentAllocations { get; set; } = new List<SubscriptionPaymentAllocation>();
		public ICollection<InvoicePaymentAttempt> InvoicePaymentAttempts { get; set; } = new List<InvoicePaymentAttempt>();
		public ICollection<InvoicePaymentAllocation> InvoicePaymentAllocations { get; set; } = new List<InvoicePaymentAllocation>();
		public ICollection<PayoutBatch> PayoutBatchs { get; set; } = new List<PayoutBatch>();
		public ICollection<PayoutItem> PayoutItems { get; set; } = new List<PayoutItem>();
		public ICollection<PayrollDisbursement> PayrollDisbursements { get; set; } = new List<PayrollDisbursement>();
		public ICollection<PayrollDisbursementItem> PayrollDisbursementItems { get; set; } = new List<PayrollDisbursementItem>();


	}
}
