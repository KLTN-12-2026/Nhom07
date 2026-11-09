using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaTenant
{
	public class Invoice : ISoftDeletableEntity, IAuditableEntity
	{
		public Guid Id { get; set; }
		public string InvoiceNo { get; set; } = null!;
		public TenantInvoiceStatus Status { get; set; } = TenantInvoiceStatus.Draft;
		public DateTimeOffset? PublishedAt { get; set; }
		public DateTimeOffset BillingPeriod { get; set; }
		public DateTimeOffset PaymentDeadlineAt { get; set; }
		public decimal TotalAmount { get; set; }
		public string? Note { get; set; }

		public Guid? DeletedByStaff { get; set; }
		public Guid? UpdatedByStaff { get; set; }
		public Guid CreatedByStaff { get; set; }
		public Guid TenantId { get; set; }
		public Guid ApartmentId { get; set; }

		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public Tenant Tenant { get; set; } = null!;
		public Apartment Apartment { get; set; } = null!;
		public User StaffCreated { get; set; } = null!;
		public User? StaffUpdated{ get; set; }
		public User? StaffDeleted { get; set; }


		public ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
		public ICollection<InvoicePaymentAttempt> InvoicePaymentAttempts { get; set; } = new List<InvoicePaymentAttempt>();

	}
}
