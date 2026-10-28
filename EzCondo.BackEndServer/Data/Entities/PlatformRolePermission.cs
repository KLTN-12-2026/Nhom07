namespace EzCondo.BackEndServer.Data.Entities
{
	public class PlatformRolePermission
	{
		public Guid RoleId { get; set; }
		public string PermissionCode { get; set; }

		public PlatformRole? PlatformRole { get; set; }
		public PlatformPermission? PlatformPermission { get; set; }
	}
}
