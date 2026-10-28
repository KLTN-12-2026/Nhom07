using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum ServiceBillingType
	{
		[PgName("fixed")]
		Fixed,

		[PgName("metered")]
		Metered,

		[PgName("one_time")]
		OneTime
	}
}
