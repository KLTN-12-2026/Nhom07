using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum PayrollStatusEnum
	{
		[PgName("draft")]
		Draft,
		[PgName("pending_approval")]
		Pending_approval,
		[PgName("processing")]
		Processing,
		[PgName("completed")]
		Completed,
		[PgName("approved")]
		Approved,
		[PgName("rejected")]
		Rejected,
		[PgName("issued")]
		Issued
	}
}
