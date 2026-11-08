using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum ProcessingStatusEnum
	{
		[PgName("received")]
		Received,
		[PgName("processing")]
		Processing,
		[PgName("succeeded")]
		Succeeded,
		[PgName("failed")]
		Failed
	}
}
