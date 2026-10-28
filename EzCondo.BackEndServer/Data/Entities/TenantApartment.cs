using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class TenantApartment : IAuditableEntity
	{
		public Guid Id { get; set; }
		public Decimal AreaM2 { get; set; }
		public string ApartmentNo { get; set; }
		public int Bedroom { get; set; }
		public ApartmentStatusEnum Status { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }
		public Guid BuildingId { get; set; }
		public TenantBuilding TenantBuilding { get; set; }

		public ICollection<ApartmentResident> ApartmentResidents { get; set; } = new List<ApartmentResident>();
		public ICollection<TenantInvoice> TenantInvoices { get; set; } = new List<TenantInvoice>();
	}
}
