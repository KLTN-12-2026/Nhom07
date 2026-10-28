using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum BillingPeriodStatus
	{
		[PgName("draft")]
		Draft,

		[PgName("collecting_readings")]
		CollectingReadings,

		[PgName("calculated")]
		Calculated,

		[PgName("issued")]
		Issued,

		[PgName("closed")]
		Closed,

		[PgName("cancelled")]
		Cancelled
	}
}
