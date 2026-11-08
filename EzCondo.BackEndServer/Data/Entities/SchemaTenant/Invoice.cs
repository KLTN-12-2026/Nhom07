using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;

namespace EzCondo.BackEndServer.Data.Entities.SchemaTenant
{
	public class Invoice : ISoftDeletableEntity, IAuditableEntity
	{
		public Guid Id { get; set; }
		public string InvoiceNo { get; set; }
		public TenantInvoiceStatus Status { get; set; }
		public DateTimeOffset? PublishedAt { get; set; }
		public DateTimeOffset BillingPeriod { get; set; }
		public DateTimeOffset PaymentDealineAt { get; set; }
		public decimal TotalAmount { get; set; }
		public string? Note { get; set; }

		public Guid? PaidByResident { get; set; }
		public Guid? DeletedByStaff { get; set; }
		public Guid? UpdatedByStaff { get; set; }
		public Guid CreatedByStaff { get; set; }
		public Guid TenantId { get; set; }
		public Guid ApartmentId { get; set; }

		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public Tenant Tenant { get; set; }//
		public Apartment Apartment { get; set; }//
		public User StaffCreated { get; set; }
		public User? StaffUpdated{ get; set; }
		public User? StaffDeleted { get; set; }
		public User? ResidentPaid { get; set; }


		public ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();

	}
}
