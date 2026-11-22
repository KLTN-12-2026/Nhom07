using EzCondo.BackEndServer.Contracts.SubscriptionRenewals;
using EzCondo.BackEndServer.Infrastructure.Authentication;
using EzCondo.BackEndServer.Services.SubscriptionRenewals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EzCondo.BackEndServer.Controllers
{
	[ApiController]
	[Authorize(Policy = AuthPolicies.TenantScope)]
	[Route("api/tenant/subscription-renewals")]
	public sealed class TenantSubscriptionRenewalsController(
		ITenantSubscriptionRenewalService renewalService)
		: ControllerBase
	{
		[HttpGet("current")]
		[ProducesResponseType<CurrentSubscriptionRenewalResponse>(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<CurrentSubscriptionRenewalResponse>> GetCurrent(
			CancellationToken cancellationToken)
		{
			var result = await renewalService.GetCurrentAsync(cancellationToken);

			if (result.IsSuccess)
			{
				return Ok(result.Value);
			}

			return result.Failure switch
			{
				GetCurrentSubscriptionRenewalFailure
					.InvalidIdentity =>
						Unauthorized(CreateProblem(
							StatusCodes.Status401Unauthorized,
							"invalid_tenant_identity",
							"Không xác định được tài khoản hoặc tenant hiện tại.")),

				GetCurrentSubscriptionRenewalFailure
					.TenantAdminRequired =>
						StatusCode(
							StatusCodes.Status403Forbidden,
							CreateProblem(
								StatusCodes.Status403Forbidden,
								"tenant_admin_required",
								"Chỉ Chủ chung cư được xem yêu cầu gia hạn.")),

				GetCurrentSubscriptionRenewalFailure
					.RenewalRequestNotFound =>
						NotFound(CreateProblem(
							StatusCodes.Status404NotFound,
							"renewal_request_not_found",
							"Không có yêu cầu gia hạn đang chờ phản hồi.")),

				GetCurrentSubscriptionRenewalFailure
					.RenewalRequestStale =>
						Conflict(CreateProblem(
							StatusCodes.Status409Conflict,
							"renewal_request_stale",
							"Yêu cầu gia hạn không còn khớp với kỳ subscription hiện tại.")),

				_ => Problem(
					statusCode: StatusCodes.Status500InternalServerError,
					title: "get_subscription_renewal_failed",
					detail: "Không thể tải yêu cầu gia hạn.")
			};
		}

		[HttpPost("{requestId:guid}/confirm")]
		[ProducesResponseType<ConfirmSubscriptionRenewalResponse>(StatusCodes.Status200OK)]
		[ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<ConfirmSubscriptionRenewalResponse>> Confirm(
			Guid requestId,
			[FromBody] ConfirmSubscriptionRenewalRequest request,
			CancellationToken cancellationToken)
		{
			var result = await renewalService.ConfirmAsync(
				requestId,
				request,
				cancellationToken);

			if (result.IsSuccess)
			{
				return Ok(result.Value);
			}

			return result.Failure switch
			{
				ConfirmSubscriptionRenewalFailure
					.InvalidIdentity =>
						Unauthorized(CreateProblem(
							StatusCodes.Status401Unauthorized,
							"invalid_tenant_identity",
							"Không xác định được tài khoản hoặc tenant hiện tại.")),

				ConfirmSubscriptionRenewalFailure
					.TenantAdminRequired =>
						StatusCode(
							StatusCodes.Status403Forbidden,
							CreateProblem(
								StatusCodes.Status403Forbidden,
								"tenant_admin_required",
								"Chỉ Chủ chung cư được xác nhận gia hạn.")),

				ConfirmSubscriptionRenewalFailure
					.RenewalRequestNotFound =>
						NotFound(CreateProblem(
							StatusCodes.Status404NotFound,
							"renewal_request_not_found",
							"Không tìm thấy yêu cầu gia hạn.")),

				ConfirmSubscriptionRenewalFailure
					.SubscriptionPlanNotFound =>
						NotFound(CreateProblem(
							StatusCodes.Status404NotFound,
							"subscription_plan_not_found",
							"Không tìm thấy gói dịch vụ được chọn.")),

				ConfirmSubscriptionRenewalFailure
					.RenewalRequestAlreadyResponded =>
						Conflict(CreateProblem(
							StatusCodes.Status409Conflict,
							"renewal_request_already_responded",
							"Yêu cầu gia hạn đã được phản hồi.")),

				ConfirmSubscriptionRenewalFailure
					.RenewalRequestStale =>
						Conflict(CreateProblem(
							StatusCodes.Status409Conflict,
							"renewal_request_stale",
							"Yêu cầu gia hạn không còn khớp với subscription hiện tại.")),

				ConfirmSubscriptionRenewalFailure
					.SubscriptionPlanUnavailable =>
						Conflict(CreateProblem(
							StatusCodes.Status409Conflict,
							"subscription_plan_unavailable",
							"Gói dịch vụ được chọn hiện không còn khả dụng.")),

				_ => Problem(
					statusCode: StatusCodes.Status500InternalServerError,
					title: "confirm_subscription_renewal_failed",
					detail: "Không thể xác nhận yêu cầu gia hạn.")
			};
		}

		[HttpPost("{requestId:guid}/decline")]
		[ProducesResponseType<DeclineSubscriptionRenewalResponse>(StatusCodes.Status200OK)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
		public async Task<ActionResult<DeclineSubscriptionRenewalResponse>> Decline(
			Guid requestId,
			CancellationToken cancellationToken)
		{
			var result = await renewalService.DeclineAsync(
				requestId,
				cancellationToken);

			if (result.IsSuccess)
			{
				return Ok(result.Value);
			}

			return result.Failure switch
			{
				DeclineSubscriptionRenewalFailure.InvalidIdentity =>
					Unauthorized(new ProblemDetails
					{
						Status = 401,
						Title = "invalid_tenant_identity",
						Detail = "Không xác định được tài khoản hoặc tenant."
					}),

				DeclineSubscriptionRenewalFailure.TenantAdminRequired =>
					StatusCode(403, new ProblemDetails
					{
						Status = 403,
						Title = "tenant_admin_required",
						Detail = "Chỉ Tenant Admin được từ chối gia hạn."
					}),

				DeclineSubscriptionRenewalFailure.RenewalRequestNotFound =>
					NotFound(new ProblemDetails
					{
						Status = 404,
						Title = "renewal_request_not_found",
						Detail = "Không tìm thấy yêu cầu gia hạn."
					}),

				DeclineSubscriptionRenewalFailure.RenewalRequestAlreadyResponded =>
					Conflict(new ProblemDetails
					{
						Status = 409,
						Title = "renewal_request_already_responded",
						Detail = "Yêu cầu gia hạn đã được phản hồi."
					}),

				DeclineSubscriptionRenewalFailure.RenewalRequestStale =>
					Conflict(new ProblemDetails
					{
						Status = 409,
						Title = "renewal_request_stale",
						Detail = "Yêu cầu gia hạn không còn khớp subscription hiện tại."
					}),

				_ => Problem(
					statusCode: 500,
					title: "decline_subscription_renewal_failed",
					detail: "Không thể từ chối gia hạn.")
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
