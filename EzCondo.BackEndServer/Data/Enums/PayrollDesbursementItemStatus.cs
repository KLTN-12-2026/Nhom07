using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum PayrollDisbursementItemStatus
	{
		[PgName("ready")]
		Ready,
		[PgName("processing")]
		Processing,
		[PgName("succeeded")]
		Succeeded,
		[PgName("failed")]
		Failed
	}
}
