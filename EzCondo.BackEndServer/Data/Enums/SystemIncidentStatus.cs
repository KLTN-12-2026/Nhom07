using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum SystemIncidentStatus
	{
		[PgName("reported")]
		Reported,

		[PgName("investigating")]
		Investigating,

		[PgName("monitoring")]
		Monitoring,

		[PgName("resolved")]
		Resolved,

		[PgName("closed")]
		Closed
	}
}
