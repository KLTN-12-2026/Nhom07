using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum PayrollStatusEnum
	{
		[PgName("draft")]
		Draft,
		[PgName("pending_approval")]
		Pending_approval,
		[PgName("approved")]
		Approved,
		[PgName("rejected")]
		Rejected,
		[PgName("published")]
		Published
	}
}
