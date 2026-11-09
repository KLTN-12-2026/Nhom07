using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaSecurity
{
	public sealed class Role
	{
		public Guid Id { get; set; }
		public RoleScopeEnum Scope { get; set; }
		public Guid? TenantId { get; set; }
		public string Code { get; set; } = null!;
		public string Name { get; set; } = null!;
		public bool IsDeleted { get; set; }
		public bool IsSystem { get; set; }

		public Tenant? Tenant { get; set; }

		public ICollection<RolePermission> RolePermissions { get; set; }
			= new List<RolePermission>();
		public ICollection<UserRole> UserRoles { get; set; }
			= new List<UserRole>();
		public ICollection<Invitation> Invitations { get; set; }
			= new List<Invitation>();
	}
}
