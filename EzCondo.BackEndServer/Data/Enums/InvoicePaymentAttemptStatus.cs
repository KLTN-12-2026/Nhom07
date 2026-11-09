using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum InvoicePaymentAttemptStatus
	{
		[PgName("pending")]
		Pending,
		[PgName("partially_paid")]
		Partially_paid,
		[PgName("paid")]
		Paid,
		[PgName("cancelled")]
		Cancelled,
		[PgName("failed")]
		Failed
	}
}
