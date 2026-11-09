using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum ProviderOwnerScope
	{
		[PgName("platform")]
		Platform,
		[PgName("tenant")]
		Tenant
	}
}
