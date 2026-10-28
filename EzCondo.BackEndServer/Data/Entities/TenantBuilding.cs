namespace EzCondo.BackEndServer.Data.Entities
{
	public class TenantBuilding
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string Address { get; set; }
		public DateTimeOffset CreatedAt { get; set; }

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }

		public ICollection<TenantApartment> TenantApartments { get; set; } = new List<TenantApartment>();
	}
}
 