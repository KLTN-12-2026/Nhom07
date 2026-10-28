namespace EzCondo.BackEndServer.Data.Entities
{
	public class TenantRolePermission
	{
		public Guid RoleId { get; set; }
		public string PermissionCode { get; set; } //Khóa ngoại đến Code của TenantPermission

		public TenantRole Role { get; set; }
		public TenantPermission TenantPermission { get; set; }
		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }
	}
}
