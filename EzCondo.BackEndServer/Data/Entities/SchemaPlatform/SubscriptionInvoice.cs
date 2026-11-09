using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaPlatform
{
	public sealed class SubscriptionInvoice : IAuditableEntity //Thanh toán subscription
	{
		public Guid Id { get; set; }
		public Guid TenantId { get; set; }
		public Guid SubscriptionId { get; set; }
		public string InvoiceNo { get; set; } = null!;
		public DateOnly? BillingPeriodFrom { get; set; }
		public DateOnly? BillingPeriodTo { get; set; }
		public DateTimeOffset DueAt{ get; set; }
		public SubscriptionInvoiceStatus Status { get; set; } = SubscriptionInvoiceStatus.Issued;
		public decimal TotalAmount { get; set; }
		public decimal PaidAmount { get; set; }
		public string Currency { get; set; } = null!;
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public Tenant Tenant { get; set; } = null!;
		public Subscription Subscription { get; set; } = null!;

		public ICollection<SubscriptionPaymentAttempt> SubscriptionPaymentAttempts { get; set; } = new List<SubscriptionPaymentAttempt>();

	}
}
