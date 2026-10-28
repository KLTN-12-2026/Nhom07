using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class TenantService : ISoftDeletableEntity, IAuditableEntity
	{
		public Guid Id { get; set; }
		public string Code { get; set; }
		public string Name { get; set; }
		public ServiceBillingType BillingType { get; set; }
		public string Unit { get; set; }
		public bool IsActive { get; set; }
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }
		public Guid? DeletedBy { get; set; }
		public Guid? UpdatedBy { get; set; }

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }

		public ICollection<TenantInvoiceLine> TenantInvoiceLines { get; set; } = new List<TenantInvoiceLine>();

	}
}
