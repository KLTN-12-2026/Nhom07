using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaSecurity
{
	public class Role
	{
		public Guid Id { get; set; }
		public TenantRoleCodeEnum Code { get; set; }
		public string Name { get; set; }
		public RoleScopeEnum RoleScope { get; set; }
		public short HierarchyLevel { get; set; }
		public bool IsDeleted { get; set; }
		public bool IsSystem { get; set; }
		public Guid? TenantId { get; set; }
		public Tenant? Tenant { get; set; }

		public ICollection<User> Users { get; set; } = new List<User>();
		public ICollection<RolePermission> RolePermissions = new List<RolePermission>();
		public ICollection<UserRole> UserRoles = new List<UserRole>();
		public ICollection<Invitation> Invitations = new List<Invitation>();

	}
}
