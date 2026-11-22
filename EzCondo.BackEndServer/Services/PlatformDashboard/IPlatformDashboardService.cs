using EzCondo.BackEndServer.Contracts.PlatformDashboard;

namespace EzCondo.BackEndServer.Services.PlatformDashboard
{
	public interface IPlatformDashboardService
	{
		Task<PlatformDashboardResponse> GetAsync(
			PlatformDashboardQuery query,
			CancellationToken cancellationToken = default);
	}
}
