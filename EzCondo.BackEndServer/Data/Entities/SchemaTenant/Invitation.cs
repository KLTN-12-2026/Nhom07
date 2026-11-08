using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaSecurity;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;

namespace EzCondo.BackEndServer.Data.Entities.SchemaTenant
{
	public class Invitation : ISoftDeletableEntity
	{
		public Guid Id { get; set; }
		public string Email { get; set; }
		public string TokenHash { get; set; }
		public DateTimeOffset? AcceptedAt { get; set; }
		public DateTimeOffset ExpiresAt { get; set; }
		public DateTimeOffset? RevokedAt { get; set; }
		public Guid CreatedBy { get; set; }
		public Guid RoleId { get; set; }
		public Guid TenantId { get; set; }
		public Guid? TargetUserId { get; set; }
		public InvitationStatusEnum Status { get; set; }

		public DateTimeOffset CreatedAt { get; set; }
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }

		public Tenant Tenant { get; set; }
		public Role Role { get; set; }
		public User User { get; set; }
		public User? TargetUser { get; set; }
	}
}
