using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum UserStatusEnum
	{
		[PgName("active")]
		Active,
		[PgName("locked")]
		Locked,
		[PgName("inviting")]
		Inviting
	}
}
