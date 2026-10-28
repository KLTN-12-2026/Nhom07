using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum TenantStatusEnum
	{
		[PgName("trial")]
		Trial,

		[PgName("active")]
		Active,

		[PgName("suspended")]
		Suspended,

		[PgName("cancelled")]
		Cancelled
	}
}
