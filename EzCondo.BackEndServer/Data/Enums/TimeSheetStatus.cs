using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum TimeSheetStatus
	{
		[PgName("draft")]
		Draft,
		[PgName("comfirmed")]
		Comfirmed,
		[PgName("included")]
		Included //Đã tính vào lương
	}
}
