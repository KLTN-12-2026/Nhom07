using EzCondo.BackEndServer.Contracts.Authentication.Platform;
using EzCondo.BackEndServer.Contracts.Authentication.Tenant;

namespace EzCondo.BackEndServer.Services.Authentication
{
	public interface IAuthService
	{
		Task<PlatformLoginResult> LoginPlatformAsync(
			PlatformLoginRequest request,
			CancellationToken cancellationToken = default);

		Task<TenantLoginResult> LoginTenantAsync(
			TenantLoginRequest request,
			CancellationToken cancellationToken = default);

		Task<LogoutResult> LogoutAllAsync(
			Guid userId,
			CancellationToken cancellationToken = default);
	}
}
