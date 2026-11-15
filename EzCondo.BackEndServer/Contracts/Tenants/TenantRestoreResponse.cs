using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Contracts.Tenants
{
	public sealed record TenantRestoreResponse
	(
		Guid TenantId,
		TenantStatusEnum Status,
		DateTimeOffset RestoredAt
	);
}
