using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum RoleScopeEnum
	{
		[PgName("platform")]
		Platform,
		[PgName("tenant")]
		Tenant
	}
}
