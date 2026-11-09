using EzCondo.BackEndServer.Contracts.Tenants;

namespace EzCondo.BackEndServer.Services.Tenants
{
	public interface ITenantService
	{
		Task<CreateTenantResult> CreateAsync(
			CreateTenantRequest request,
			CancellationToken cancellationToken = default);
	}
}
