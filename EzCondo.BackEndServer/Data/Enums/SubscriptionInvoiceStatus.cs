using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum SubscriptionInvoiceStatus
	{
		[PgName("issued")]
		Issued,
		[PgName("partially_paid")]
		Partially_paid,
		[PgName("paid")]
		Paid,
		[PgName("overdue")]
		OverDue,
		[PgName("cancelled")]
		Cancelled
	}
}
