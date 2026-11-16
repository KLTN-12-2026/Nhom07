using EzCondo.BackEndServer.Contracts.TenantAdminInvitations;

namespace EzCondo.BackEndServer.Services.TenantAdminInvitations
{
	public interface ITenantAdminInvitationService
	{
		Task<CreateTenantAdminInvitationResult> CreateAsync(
			Guid tenantId,
			CreateTenantAdminInvitationRequest request,
			CancellationToken cancellationToken = default);
	}
}
