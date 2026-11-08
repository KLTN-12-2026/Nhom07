using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaSecurity
{
	public class Permission
	{
		public string Code { get; set; }//key
		public string Description { get; set; }
		public RoleScopeEnum Scope { get; set; }
		public bool IsDeleted { get; set; }

		public ICollection<RolePermission> RolePermissions { get; set; }
	}
}
