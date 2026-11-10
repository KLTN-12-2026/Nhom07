using EzCondo.BackEndServer.Contracts.Tenants;
using EzCondo.BackEndServer.Services.Tenants;
using Microsoft.AspNetCore.Mvc;

namespace EzCondo.BackEndServer.Controllers
{
	[ApiController]
	[Route("api/platform/tenants")]
	public sealed class TenantController(
		ITenantService tenantService) 
		: Controller
	{
		[HttpPost]
		[ProducesResponseType<TenantResponse>(
			StatusCodes.Status201Created)]
		[ProducesResponseType<ProblemDetails>(
			StatusCodes.Status404NotFound)]
		[ProducesResponseType<ProblemDetails>(
			StatusCodes.Status409Conflict)]
		public async Task<ActionResult<TenantResponse>> Create(
			[FromBody] CreateTenantRequest request,
			CancellationToken cancellationToken)
		{
			var result = await tenantService.CreateAsync(
				request,
				cancellationToken);

			if (result.IsSuccess)
			{
				var tenant = result.Value!;

				return Created(
					 $"/api/platform/tenants/{tenant.Id}",
					tenant);
			}

			return result.Failure switch
			{
				CreateTenantFailure.SubscriptionPlanNotFound =>
					NotFound(CreateProblem(
						StatusCodes.Status404NotFound,
						"Subscription_plan_not_found",
						"Không tìm thấy gói dịch vụ.")),

				CreateTenantFailure.SubscriptionPlanUnavailable =>
					NotFound(CreateProblem(
						StatusCodes.Status409Conflict,
						"Subscription_plan_unavailable",
						"Gói dịch vụ đã ngừng cung cấp.")),

				CreateTenantFailure.TaxCodeAlreadyExists =>
					NotFound(CreateProblem(
						StatusCodes.Status409Conflict,
						"tenant_tax_code_exists",
						"Mã số thuế đã được một tenant khác sử dụng.")),

				CreateTenantFailure.GeneratedidentityCollision =>
					Problem(
						statusCode:
							StatusCodes.Status500InternalServerError,
						title: "Không thể sinh định danh tenant.",
						detail:
						"Vui lòng thực hiện lại yêu cầu."),

				_ => Problem(
					statusCode:
						StatusCodes.Status500InternalServerError,
					title: "Không thể khởi tạo tenant.")
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

		[HttpPost("{id:guid}/suspension")]
		[ProducesResponseType<TenantSuspensionResponse>(
			StatusCodes.Status202Accepted)]
		[ProducesResponseType<ProblemDetails>(
			StatusCodes.Status404NotFound)]
		[ProducesResponseType<ProblemDetails>(
			StatusCodes.Status409Conflict)]
		public async Task<ActionResult<TenantSuspensionResponse>> ScheduleSuspension(
			Guid id,
			[FromBody] ScheduleTenantSuspensionRequest request,
			CancellationToken cancellationToken)
		{
			var result = await tenantService.ScheduleSuspensionAsync(
				id,
				request,
				cancellationToken);

			if (result.IsSuccess)
			{
				return Accepted(result.Value);
			}

			return result.Failure switch
			{
				ScheduleTenantSuspensionFailure.TenantNotFound =>
				NotFound(CreateProblem(
					StatusCodes.Status404NotFound,
					"tenant_not_found",
					"Không tìm thấy tenant.")),

				ScheduleTenantSuspensionFailure.TenantAlreadySuspended =>
				Conflict(CreateProblem(
					StatusCodes.Status409Conflict,
					"tenant_already_suspended",
					"Tenant đã bị tạm ngưng.")),

				ScheduleTenantSuspensionFailure.TenantClosed =>
				Conflict(CreateProblem(
					StatusCodes.Status409Conflict,
					"tenant_closed",
					"Tenant đã đóng.")),

				ScheduleTenantSuspensionFailure.SuspensionAlreadyScheduled =>
				Conflict(CreateProblem(
					StatusCodes.Status409Conflict,
					"tenant_suspension_already_scheduled",
					"Tenant đã có lịch tạ ngưng.")),

				_ => Problem(
					statusCode: StatusCodes.Status500InternalServerError,
					title: "Không thể lên lịch tạm ngưng tenant.")
			};
		}

		[HttpPost("{id:guid}/restore")]
		[ProducesResponseType<TenantRestoreResponse>(
			StatusCodes.Status200OK)]
		[ProducesResponseType<ProblemDetails>(
			StatusCodes.Status404NotFound)]
		[ProducesResponseType<ProblemDetails>(
			StatusCodes.Status409Conflict)]
		public async Task<ActionResult<TenantRestoreResponse>> Restore(
			Guid id,
			CancellationToken cancellationToken)
		{
			var result = await tenantService.RestoreAsync(
				id,
				cancellationToken);

			if (result.IsSuccess)
			{
				return Ok(result.Value);
			}

			return result.Failure switch
			{
				RestoreTenantFailure.TenantNotFound =>
					NotFound(CreateProblem(
						StatusCodes.Status404NotFound,
						"tenant_not_found",
						"Không tìm thấy tenant.")),

				RestoreTenantFailure.TenantNotSuspended =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"tenant_not_suspended",
						"Tenant hiện không bị tạm ngưng.")),

				RestoreTenantFailure.NoValidSubscription =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"tenant_subscription_invalid",
						"Tenant không còn trial hoặc subscription hợp lệ.")),

				_ => Problem(
					statusCode: StatusCodes.Status500InternalServerError,
					title: "Không thể khôi phục tenant.")
			};
		}
	}
}
