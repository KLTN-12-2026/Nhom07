using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum PaymentTransactionStatus
	{
		[PgName("received")]
		Received,

		[PgName("verified")]
		Verified,

		[PgName("rejected")]
		Rejected,

		[PgName("refunded")]
		Refunded
	}
}
