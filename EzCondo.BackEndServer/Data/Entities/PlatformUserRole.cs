namespace EzCondo.BackEndServer.Data.Entities
{
	public class PlatformUserRole
	{
		public Guid UserId { get; set; }
		public Guid RoleId { get; set; }
		public Guid? GrantedBy { get; set; }
		public DateTimeOffset GrantedAt { get; set; }
		
		public IdentityUser User { get; set; }
		public PlatformRole PlatformRole { get; set; }
		public IdentityUser? GrantedByUser { get; set; }
	}
}
