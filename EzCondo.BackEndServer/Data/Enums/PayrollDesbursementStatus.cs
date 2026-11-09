using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum PayrollDisbursementStatus
	{
		[PgName("ready")]
		Ready,
		[PgName("issuing")]
		Issuing,
		[PgName("processing")]
		Processing,
		[PgName("partially_succeeded")]
		Partially_succeeded,
		[PgName("succeeded")]
		Succeeded,
		[PgName("failed")]
		Failed
	}
}
