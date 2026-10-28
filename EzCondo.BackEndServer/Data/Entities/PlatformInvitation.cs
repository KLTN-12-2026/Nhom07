namespace EzCondo.BackEndServer.Data.Entities
{
	public class PlatformInvitation : ISoftDeletableEntity, IAuditableEntity
	{
		public Guid Id { get; set; }
		public string Email { get; set; }
		public string TokenHash { get; set; }
		public DateTimeOffset ExpiresAt { get; set; }
		public DateTimeOffset? AcceptedAt { get; set; }
		public DateTimeOffset? RevokedAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }

		public Guid PlatformRoleId { get; set; }
		public PlatformRole PlatformRole { get; set; }
		public Guid InvitedBy { get; set; }
		public IdentityUser User { get; set; }
	}
}
