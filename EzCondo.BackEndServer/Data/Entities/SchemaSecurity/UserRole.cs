using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaSecurity
{
	public sealed class UserRole
	{
		public Guid Id { get; set; }
		public RoleScopeEnum Scope { get; set; }
		public Guid? TenantId { get; set; }
		public Guid UserId { get; set; }
		public Guid RoleId { get; set; }
		public Guid? GrantedByUserId { get; set; }
		public DateTimeOffset GrantedAt { get; set; }
		public DateTimeOffset? RevokedAt { get; set; }

		public Tenant? Tenant { get; set; }
		public User User { get; set; } = null!;
		public Role Role { get; set; } = null!;
		public User? GrantedByUser { get; set; }
	}
}
