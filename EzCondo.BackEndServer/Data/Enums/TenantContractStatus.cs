using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum TenantContractStatus
	{
		[PgName("active")]
		Active,
		[PgName("expired")]
		Expired,
		[PgName("terminated")]
		Teminated
	}
}
