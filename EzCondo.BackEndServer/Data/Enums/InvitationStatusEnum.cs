using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum InvitationStatusEnum
	{
		[PgName("accepted")]
		Accepted,
		[PgName("sent")]
		Sent,
		[PgName("revoked")]
		Revoked,
		[PgName("expired")]
		Expired,
		[PgName("pending_delivery")] //Invitation đã được tạo nhưng gmail chưa xác nhận.
		PendingDelivery
	}
}
