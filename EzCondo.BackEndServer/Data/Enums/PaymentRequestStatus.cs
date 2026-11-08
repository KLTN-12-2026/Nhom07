using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum PaymentRequestStatus
	{
		[PgName("pending")]
		Pending,
		[PgName("underpaid")]
		Underpaid,
		[PgName("paid")]
		Paid,
		[PgName("cancelled")]
		Cancelled,
		[PgName("expired")]
		Expired,
		[PgName("failed")]
		Failed,
	}
}
