using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum PayoutItemStatusEnum
	{
		[PgName("created")]
		Created,
		[PgName("processing")]
		Processing,
		[PgName("succeeded")]
		Succeeded,
		[PgName("failed")]
		Failed,
		[PgName("cancelled")]
		Cancelled
	}
}
