using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum SupportTicketStatus
	{
		[PgName("open")]
		Open,

		[PgName("assigned")]
		Assigned,

		[PgName("in_progress")]
		InProgress,

		[PgName("resolved")]
		Resolved,

		[PgName("closed")]
		Closed,

		[PgName("cancelled")]
		Cancelled
	}
}
