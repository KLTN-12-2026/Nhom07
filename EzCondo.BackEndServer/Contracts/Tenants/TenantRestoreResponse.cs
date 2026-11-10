using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Contracts.Tenants
{
	public sealed record TenantRestoreResponse
	(
		Guid TenatId,
		TenantStatusEnum Status,
		DateTimeOffset RestoredAt
	);
}
