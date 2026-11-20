using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaPlatform
{
	public sealed class SubscriptionInvoice
		: IAuditableEntity, ISoftDeletableEntity
	{
		public Guid Id { get; set; }

		public Guid TenantId { get; set; }

		public Guid SubscriptionId { get; set; }

		public string InvoiceNo { get; set; } = null!;

		public DateOnly? BillingPeriodFrom { get; set; }

		public DateOnly? BillingPeriodTo { get; set; }

		public DateTimeOffset DueAt { get; set; }

		public SubscriptionInvoiceStatus Status { get; set; }
			= SubscriptionInvoiceStatus.Draft;

		public decimal TotalAmount { get; set; }

		public decimal PaidAmount { get; set; }

		public string Currency { get; set; } = "VND";

		public Guid CreatedByAdminId { get; set; }

		public Guid? IssuedByAdminId { get; set; }

		public Guid? DeletedByAdminId { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset? IssuedAt { get; set; }

		public DateTimeOffset? UpdatedAt { get; set; }

		public bool IsDeleted { get; set; }

		public DateTimeOffset? DeletedAt { get; set; }

		public Tenant Tenant { get; set; } = null!;

		public Subscription Subscription { get; set; } = null!;

		public ICollection<SubscriptionPaymentAttempt>
			SubscriptionPaymentAttempts
		{ get; set; }
				= new List<SubscriptionPaymentAttempt>();
	}
}
