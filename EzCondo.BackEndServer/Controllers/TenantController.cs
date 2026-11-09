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
	}
}
