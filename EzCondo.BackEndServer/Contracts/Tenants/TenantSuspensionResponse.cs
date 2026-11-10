using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Contracts.Tenants
{
	public sealed record TenantSuspensionResponse
	(
		Guid TenantId,
		TenantStatusEnum CurrentStatus,
		TenantSuspensionReasonCode ReasonCode,
		string Reason,
		DateTimeOffset EffectiveAt,
		DateTimeOffset? NoticeSentAt,
		DateTimeOffset? SuspendedAt);
}
