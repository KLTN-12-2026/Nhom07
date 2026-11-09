using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;

namespace EzCondo.BackEndServer.Data.Entities.SchemaTenant
{
	public class Apartment : ISoftDeletableEntity, IAuditableEntity
	{
		public Guid Id { get; set; }
		public Decimal AreaM2 { get; set; }
		public string ApartmentNo { get; set; } = null!;
		public int Bedroom { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }
		
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public Guid TenantId { get; set; }
		public Guid ApartmentRenter { get; set; }

		public Tenant Tenant { get; set; } = null!;
		public User Resident { get; set; } = null!;

		public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
		public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
	}
}
