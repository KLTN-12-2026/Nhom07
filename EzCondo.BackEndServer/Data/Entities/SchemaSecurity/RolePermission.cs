using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaSecurity
{
	public class RolePermission
	{
		public Guid RoleId { get; set; }
		public string PermissionCode { get; set; } //Khóa ngoại đến Code của Permission
		public RoleScopeEnum Scope { get; set; }

		public Role Role { get; set; }
		public Permission Permission { get; set; }
	}
}
