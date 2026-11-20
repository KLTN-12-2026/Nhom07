using EzCondo.BackEndServer.Contracts.TenantAdminInvitations;
using EzCondo.BackEndServer.Infrastructure.Authentication;
using EzCondo.BackEndServer.Services.TenantAdminInvitations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EzCondo.BackEndServer.Controllers
{
	[ApiController]
	[Authorize(Policy = AuthPolicies.PlatformScope)]
	[Route("api/platform/tenants/{tenantId:guid}/tenant-admin-invitations")]
	public sealed class TenantAdminInvitationsController(
		ITenantAdminInvitationService invitationService)
		: ControllerBase
	{
		[HttpPost]
		[ProducesResponseType<TenantAdminInvitationResponse>(StatusCodes.Status201Created)]
		[ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<TenantAdminInvitationResponse>> Create(
			Guid tenantId,
			[FromBody] CreateTenantAdminInvitationRequest request,
			CancellationToken cancellationToken)
		{
			var result = await invitationService.CreateAsync(
				tenantId,
				request,
				cancellationToken);

			if (result.IsSuccess)
			{
				return StatusCode(
					StatusCodes.Status201Created,
					result.Value);
			}

			return result.Failure switch
			{
				CreateTenantAdminInvitationFailure.TenantNotFound =>
					NotFound(CreateProblem(
						StatusCodes.Status404NotFound,
						"tenant_not_found",
						"Không tìm thấy tenant.")),

				CreateTenantAdminInvitationFailure.TenantUnavailable =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"tenant_unavailable",
						"Tenant hiện không hoạt động hoặc thời gian dùng thử đã hết.")),


				CreateTenantAdminInvitationFailure.TenantAdminAlreadyExists =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"tenant_admin_already_exists",
						"Tenant đã có Chủ chung cư đang hoạt động.")),

				CreateTenantAdminInvitationFailure.ActiveInvitationAlreadyExists =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"active_invitation_already_exists",
						"Tenant đã có lời mời Chủ chung cư đang hoạt động.")),

				CreateTenantAdminInvitationFailure.EmailAlreadyAssignedToAnotherAccount =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"email_already_assigned",
						"Email đã được sử dụng bởi một tài khoản khác.")),

				CreateTenantAdminInvitationFailure.TenantAdminRoleNotFound =>
					Problem(
						statusCode:
							StatusCodes.Status500InternalServerError,
						title:
							"tenant_admin_role_not_found",
						detail:
							"Tenant chưa được cấu hình role TENANT_ADMIN."),

				_=> Problem(
					statusCode:
						StatusCodes.Status500InternalServerError,
					title:
						"tenant_admin_invitation_failed",
					detail:
						"Không thể tạo lời mời Chủ chung cư.")
			};
		}

		private static ProblemDetails CreateProblem(
			int status, 
			string code, 
			string detail)
		{
			return new ProblemDetails
			{
				Status = status,
				Title = code,
				Detail = detail
			};
		}
	}
}
