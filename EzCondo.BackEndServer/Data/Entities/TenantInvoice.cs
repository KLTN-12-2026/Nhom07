using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class TenantInvoice : ISoftDeletableEntity, IAuditableEntity
	{
		public Guid Id { get; set; }
		public string InvoiceNo { get; set; }
		public TenantInvoiceStatus Status { get; set; }
		public string Currency { get; set; }
		public Decimal SubtotalAmount { get; set; }
		public Decimal AdjustmentAmount { get; set; }
		public Decimal TotalAmount { get; set; }
		public Decimal PaidAmount { get; set; }
		public DateTimeOffset? IssuedAt { get; set; }
		public DateTimeOffset? DueAt { get; set; }
		public string? Note { get; set; }
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }
		public Guid? DeletedBy { get; set; }
		public Guid? UpdatedBy { get; set; }
		public Guid CreatedBy { get; set; }

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }
		public Guid BillingPeriodId { get; set; }
		public TenantBillingPeriod TenantBillingPeriod { get; set; }
		public Guid ApartmentId { get; set; }
		public TenantApartment TenantApartment { get; set; }
		public Guid BillingContactId { get; set; }
		public TenantResident TenantResident { get; set; }

		public ICollection<TenantInvoiceLine> TenantInvoiceLines { get; set; } = new List<TenantInvoiceLine>();

	}
}
