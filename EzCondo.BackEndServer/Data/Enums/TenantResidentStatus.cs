using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum TenantResidentStatus
	{
		[PgName("invited")]
		Invited,

		[PgName("active")]
		Active,

		[PgName("suspended")]
		Suspended,

		[PgName("left")]
		Left
	}
}
