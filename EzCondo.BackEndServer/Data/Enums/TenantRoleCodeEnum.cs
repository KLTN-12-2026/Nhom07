using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum TenantRoleCodeEnum
	{
		[PgName("tenant_admin")]
		TENANT_ADMIN,
		[PgName("manager")]
		MANAGER,
		[PgName("staff")]
		STAFF,
		[PgName("resident")]
		RESIDENT
	}
}
