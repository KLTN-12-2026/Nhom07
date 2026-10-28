namespace EzCondo.BackEndServer.Data.Entities
{
	public class TenantUserRole
	{
		public Guid Id { get; set; }
		public DateTime GrantedAt { get; set; }
		public DateTime? RevokedAt { get; set; }

		public Guid UserId { get; set; }
		public IdentityUser User { get; set; }
		public Guid? GrantedBy { get; set; }
		public IdentityUser? GrantedUser { get; set; }
		public Guid RoleId { get; set; }
		public TenantRole Role { get; set; }
	}
}
