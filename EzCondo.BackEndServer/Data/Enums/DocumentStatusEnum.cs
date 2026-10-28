using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum DocumentStatusEnum
	{
		[PgName("draft")]
		Draft,

		[PgName("published")]
		Published,

		[PgName("archived")]
		Archived
	}
}
