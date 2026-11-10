using EzCondo.BackEndServer.Contracts.Tenants;

namespace EzCondo.BackEndServer.Services.Tenants
{
	public interface ITenantService
	{
		Task<CreateTenantResult> CreateAsync(
			CreateTenantRequest request,
			CancellationToken cancellationToken = default);

		Task<ScheduleTenantSuspensionResult> ScheduleSuspensionAsync( //Lên lịch tạm ngưng tenant
			Guid tenantId,
			ScheduleTenantSuspensionRequest request,
			CancellationToken cancellationToken = default);

		Task<RestoreTenantResult> RestoreAsync(
			Guid tenantId,
			CancellationToken cancellationToken = default);
	}
}
