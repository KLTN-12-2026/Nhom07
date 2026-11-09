using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum PayoutBatchStatus
	{
		[PgName("created")]
		Created,
		[PgName("submitting")]
		Submitting,
		[PgName("processing")]
		Processing,
		[PgName("partially_succeeded")]
		Partially_succeeded,
		[PgName("succeeded")]
		Succeeded,
		[PgName("failed")]
		Failed,
		[PgName("cancelled")]
		Cancelled
	}
}
