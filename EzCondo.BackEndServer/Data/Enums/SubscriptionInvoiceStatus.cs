using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum SubscriptionInvoiceStatus
	{
		[PgName("draft")]
		Draft,

		[PgName("issued")]
		Issued,

		[PgName("partially_paid")]
		PartiallyPaid,

		[PgName("paid")]
		Paid,

		[PgName("overdue")]
		Overdue,

		[PgName("cancelled")]
		Cancelled,

		[PgName("void")]
		Void
	}
}
