using EzCondo.BackEndServer.Contracts.Authentication.Tenant;
using EzCondo.BackEndServer.Services.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EzCondo.BackEndServer.Controllers
{
	[ApiController]
	[Route("api/tenant/auth")]
	public sealed class TenantAuthController(
		IAuthService authService,
		ILogger<TenantAuthController> logger)
		: ControllerBase
	{
		[AllowAnonymous]
		[HttpPost("login")]
		[ProducesResponseType<TenantLoginResponse>(StatusCodes.Status200OK)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<TenantLoginResponse>> Login(
			[FromBody] TenantLoginRequest request,
			CancellationToken cancellationToken)
		{
			var result = await authService.LoginTenantAsync(
				request,
				cancellationToken);

			if (result.IsSuccess)
			{
				return Ok(result.Value);
			}

			logger.LogWarning(
				"Tenant login bị từ chối. TenantSlug: {TenantSlug}, Failure: {Failure}",
				request.TenantSlug,
				result.Failure);

			return Unauthorized(
				new ProblemDetails
				{
					Status = StatusCodes.Status401Unauthorized,
					Title = "invalid_credentials",
					Detail = "Thông tin đăng nhập không hợp lệ."
				});
		}
	}
}
