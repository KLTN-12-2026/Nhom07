using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Entities.SchemaSecurity;

namespace EzCondo.BackEndServer.Data.Entities.SchemaIdentity
{
	public class UserRole
	{
		public Guid Id { get; set; }
		public Guid? TenantId { get; set; }
		public Guid NewUserId { get; set; }
		public Guid RoleId { get; set; }
		public Guid? GrantedBy { get; set; }
		public DateTimeOffset GrantedAt { get; set; }
		public DateTimeOffset? RevokedAt { get; set; }
		
		public Tenant? Tenant { get; set; }
		public User NewUser { get; set; }
		public Role Role { get; set; }
		public User? UserGranted { get; set; }
	}
}
