using EzCondo.BackEndServer.Contracts.Invitations;
using EzCondo.BackEndServer.Services.Invitations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EzCondo.BackEndServer.Controllers
{
	[ApiController]
	[AllowAnonymous]
	[Route("api/invitations")]
	public sealed class InvitationsController(
		IInvitationAcceptanceService invitationAcceptanceService)
		: ControllerBase
	{
		[HttpPost("accept")]
		[ProducesResponseType<AcceptInvitationResponse>(StatusCodes.Status200OK)]
		[ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status410Gone)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<AcceptInvitationResponse>> Accept(
			[FromBody] AcceptInvitationRequest request,
			CancellationToken cancellationToken)
		{
			var result =
				await invitationAcceptanceService.AcceptAsync(
					request,
					cancellationToken);

			if (result.IsSuccess)
			{
				return Ok(result.Value);
			}

			return result.Failure switch
			{
				AcceptInvitationFailure.InvalidToken =>
					BadRequest(CreateProblem(
						StatusCodes.Status400BadRequest,
						"invalid_invitation_token",
						"Token lời mời không hợp lệ.")),

				AcceptInvitationFailure.PasswordRejected =>
					BadRequest(CreatePasswordProblem(result.Errors)),

				AcceptInvitationFailure.InvitationNotSent =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"invitation_not_sent",
						"Lời mời chưa được gửi thành công.")),

				AcceptInvitationFailure.InvitationExpired =>
				StatusCode(
					StatusCodes.Status410Gone,
					CreateProblem(
						StatusCodes.Status410Gone,
						"invitation_expired",
						"Lời mời đã hết hạn.")),

				AcceptInvitationFailure.InvitationRevoked =>
				StatusCode(
					StatusCodes.Status410Gone,
					CreateProblem(
						StatusCodes.Status410Gone,
						"invitation_revoked",
						"Lời mời đã bị thu hồi.")),

				AcceptInvitationFailure.InvitationAlreadyAccepted =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"invitation_already_accepted",
						"Lời mời đã được sử dụng.")),

				AcceptInvitationFailure.TenantUnavailable =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"tenant_unavailable",
						"Chung cư hiện không khả dụng.")),

				AcceptInvitationFailure.TargetUserUnavailable =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"invited_account_unavailable",
						"Tài khoản được mời hiện không khả dụng.")),

				AcceptInvitationFailure.RoleUnavailable =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"invited_role_unavailable",
						"vai trò trong lời mời hiện không khả dụng.")),

				AcceptInvitationFailure.ActiveRoleAlreadyExists =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"active_role_already_exists",
						"Tài khoản hoặc tenant đã có vai trò hoạt động.")),

				_ => Problem(
					statusCode:
						StatusCodes.Status500InternalServerError,
					title: "accept_invitation_failed",
					detail: "Không thể chấp nhận lời mời.")
			};
		}

		private static ValidationProblemDetails CreatePasswordProblem(
			IReadOnlyCollection<string> errors)
		{
			return new ValidationProblemDetails(
				new Dictionary<string, string[]>
				{
					[nameof(AcceptInvitationRequest.Password)] = errors.ToArray()
				})
			{
				Status = StatusCodes.Status400BadRequest,
				Title = "password_rejected",
				Detail = "Mật khẩu không đáp ứng chính sách bảo mật."
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
