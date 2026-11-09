using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaSecurity
{
	public sealed class Permission
	{
		public RoleScopeEnum Scope { get; set; }
		public string Code { get; set; } = null!;
		public string Description { get; set; } = null!;

		public ICollection<RolePermission> RolePermissions { get; set; }
			= new List<RolePermission>();
	}
}
