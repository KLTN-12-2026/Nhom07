using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Contracts.TenantAdminInvitations
{
	public sealed record TenantAdminInvitationResponse
	(
		Guid InvitationId,
		Guid TenantId,
		Guid TargetUserId,
		string Email,
		string FullName,
		InvitationStatusEnum Status,
		DateTimeOffset ExpiresAt,
		DateTimeOffset? EmailSentAt,
		int EmailSendAttemptCount
		);
}
