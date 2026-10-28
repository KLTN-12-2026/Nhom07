using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum DocumentTypeEnum
	{
		[PgName("regulation")]
		Regulation,
		[PgName("rule")]
		Rule,
		[PgName("faq")]
		Faq,
		[PgName("service_guide")]
		Service_guide,
		[PgName("notice")]
		Notice
	}
}
