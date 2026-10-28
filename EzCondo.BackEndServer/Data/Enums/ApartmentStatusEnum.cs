using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum ApartmentStatusEnum
	{
		[PgName("available")]
		Available,

		[PgName("occupied")]
		Occupied,

		[PgName("reserved")]
		Reserved,

		[PgName("maintenance")]

		Maintenance,

		[PgName("locked")]
		Locked
	}
}
