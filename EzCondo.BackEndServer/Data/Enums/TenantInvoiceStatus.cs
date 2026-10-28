using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum TenantInvoiceStatus
	{
		[PgName("draft")]
		Draft,

		[PgName("issued")]
		Issued,
		
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
