using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum ResidentComplaintStatus
	{
		[PgName("assigned")]
		Assigned,

		[PgName("in_progress")]
		InProgress,

		[PgName("resolved")]
		Resolved,

		[PgName("cancelled")]
		Cancelled
	}
}
