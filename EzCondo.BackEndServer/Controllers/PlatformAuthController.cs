using EzCondo.BackEndServer.Contracts.Authentication.Platform;
using EzCondo.BackEndServer.Infrastructure.Authentication;
using EzCondo.BackEndServer.Services.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EzCondo.BackEndServer.Controllers
{
	[ApiController]
	[Authorize(Policy = AuthPolicies.PlatformScope)]
	[Route("api/platform/auth")]
	public sealed class PlatformAuthController(
		IAuthService authService,
		ILogger<PlatformAuthController> logger)
		: ControllerBase
	{
		[AllowAnonymous]
		[HttpPost("login")]
		[ProducesResponseType<PlatformLoginResponse>(
			StatusCodes.Status200OK)]
		[ProducesResponseType<ProblemDetails>(
			StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<PlatformLoginResponse>> Login(
			[FromBody] PlatformLoginRequest request,
			CancellationToken cancellationToken)
		{
			var result = await authService.LoginPlatformAsync(
				request,
				cancellationToken);

			if (result.IsSuccess)
			{
				return Ok(result.Value);
			}

			logger.LogWarning(
				"Platform login bị từ chối. Failure: {Failure}",
				result.Failure);

			return Unauthorized(
				CreateProblem(
					StatusCodes.Status401Unauthorized,
					"Invalid_credentials",
					"Thông tin đăng nhập không hợp lệ."));
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

		[Authorize(Policy = AuthPolicies.PlatformScope)]
		[HttpPost("logout")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> Logout(
		CancellationToken cancellationToken)
		{
			var subject = User.FindFirstValue(AppClaimTypes.Subject);

			if (!Guid.TryParse(subject, out var userId))
			{
				return Unauthorized(
					CreateProblem(
						StatusCodes.Status401Unauthorized,
						"invalid_token",
						"Token không chứa thông tin người dùng hợp lệ."));
			}

			var result = await authService.LogoutAllAsync(userId, cancellationToken);

			if (result.IsSuccess)
			{
				return NoContent();
			}

			return result.Failure switch
			{
				LogoutFailure.UserNotFound =>
					Unauthorized(
						CreateProblem(
							StatusCodes.Status401Unauthorized,
							"account_not_found",
							"Không xác định được tài khoản đăng nhập.")),

				LogoutFailure.UpdateSecurityStampFailed =>
					Problem(
						statusCode:
							StatusCodes.Status500InternalServerError,
						title:
							"Không thể đăng xuất tài khoản."),

				_ => Problem(
					statusCode:
						StatusCodes.Status500InternalServerError,
					title:
						"Không thể đăng xuất tài khoản.")
			};
		}
	}
}
