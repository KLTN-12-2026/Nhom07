using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum SubscriptionStatusEnum
	{
		[PgName("trial")]
		Trial,

		[PgName("active")]
		Active,

		[PgName("past_due")]
		PastDue,

		[PgName("suspended")]
		Suspended,

		[PgName("cancelled")]
		Cancelled,

		[PgName("expired")]
		Expired
	}
}
