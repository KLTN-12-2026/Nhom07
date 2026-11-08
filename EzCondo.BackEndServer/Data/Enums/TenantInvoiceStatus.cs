using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum TenantInvoiceStatus
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
		Published,
		[PgName("paid")]
		Paid,
		[PgName("unpaid")]
		Unpaid
	}
}
