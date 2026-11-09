using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaSecurity
{
	public sealed class RolePermission
	{
		public RoleScopeEnum Scope { get; set; }
		public Guid RoleId { get; set; }
		public string PermissionCode { get; set; } = null!;

		public Role Role { get; set; } = null!;
		public Permission Permission { get; set; } = null!;
	}
}
