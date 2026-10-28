namespace EzCondo.BackEndServer.Data.Entities
{
	public class TenantRole
	{
		public Guid Role { get; set; }
		public string Code { get; set; }
		public string Name { get; set; }
		public int HierarchyLevel { get; set; }
		public bool IsSystem { get; set; }
		public DateTimeOffset CreatedAt { get; set; }

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }

		public ICollection<TenantUserRole> TenantUserRoles { get; set; } = new List<TenantUserRole>();
	}
}
