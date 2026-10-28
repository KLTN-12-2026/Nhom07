using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum AiRoleEnum
	{
		[PgName("user")]
		User,
		[PgName("assistant")]
		Assistant,
		[PgName("system")]
		system
	}
}
