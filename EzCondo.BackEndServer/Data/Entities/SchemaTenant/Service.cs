using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;

namespace EzCondo.BackEndServer.Data.Entities.SchemaTenant
{
	public class Service : ISoftDeletableEntity, IAuditableEntity
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = null!;
		public string Name { get; set; } = null!;
		public decimal Price { get; set; }

		public Guid CreatedByManager { get; set; }
		public Guid? DeletedByManager { get; set; }
		public Guid? UpdatedByManager { get; set; }
		public Guid TenantId { get; set; }

		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }
		
		public Tenant Tenant { get; set; } = null!;
		public User ManagerCreated { get; set; } = null!;
		public User? ManagerUpdated { get; set; }
		public User? ManagerDeleted { get; set; }

		public ICollection<InvoiceDetail> TenantInvoiceLines { get; set; } = new List<InvoiceDetail>();

	}
}
