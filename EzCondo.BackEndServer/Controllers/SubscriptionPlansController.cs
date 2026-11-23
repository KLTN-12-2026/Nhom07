using EzCondo.BackEndServer.Contracts.SubscriptionPlans;
using EzCondo.BackEndServer.Services.SubscriptionPlans;
using Microsoft.AspNetCore.Mvc;
using EzCondo.BackEndServer.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace EzCondo.BackEndServer.Controllers
{
	[ApiController]
	[Authorize(Policy = AuthPolicies.PlatformScope)]
	[Route("api/platform/subscription-plans")]
	public class SubscriptionPlansController(
		ISubscriptionPlanService subscriptionService) 
		: ControllerBase
	{
		[HttpPost]
		public async Task<ActionResult<SubscriptionPlanResponse>> Create(
			[FromBody] CreateSubscriptionPlanRequest request,
			CancellationToken cancellationToken)
		{
			var result = await subscriptionService.CreateAsync(
				request,
				cancellationToken);

			return Created(
				$"/api/platform/subscription-plans/{result.Id}",
				result);
		}

		[HttpPut("{id:guid}")]
		public async Task<ActionResult<SubscriptionPlanResponse>> Update(
			Guid id,
			[FromBody] UpdateSubscriptionPlanRequest request,
			CancellationToken cancellationToken)
		{
			var result = await subscriptionService.UpdateAsync(
				id,
				request,
				cancellationToken);

			if(result == null)
			{
				return NotFound();
			}

			return Ok(result);
		}

		[HttpGet]
		public async Task<ActionResult<IReadOnlyList<SubscriptionPlanResponse>>> Get(CancellationToken cancellationToken)
		{
			var result = await subscriptionService.GetAsync(cancellationToken);

			return Ok(result);
		}

		//Vấn đề: sub có thể đang được sử dụng bởi 1 tenant? -> IsAvailable=false : Ngừng kinh doanh
		[HttpDelete("{id:guid}")]
		public async Task<IActionResult> Delete(
			Guid id,
			CancellationToken cancellationToken)
		{
			//Kiểm tra trước: có tenant nào đang sử dụng gói này ko
			var deleted = await subscriptionService.DeleteAsync(
				id,
				cancellationToken);

			if (!deleted)
			{
				return NotFound();
			}

			return NoContent();
		}

		[HttpPost("{id:guid}/discontinue")]
		public async Task<ActionResult<SubscriptionPlanResponse>> Discontinue(
			Guid id,
			CancellationToken cancellationToken)
		{
			var result = await subscriptionService.DiscontinueAsync(
				id,
				cancellationToken);

			if(result is null)
			{
				return NotFound();
			}

			return Ok(result);
		}

		[HttpGet("{id:guid}/tenants")]
		[ProducesResponseType<SubscriptionPlanTenantsResponse>(StatusCodes.Status200OK)]
		[ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<ActionResult<SubscriptionPlanTenantsResponse>> GetTenantsByPlan(
			Guid id,
			[FromQuery] GetSubscriptionPlanTenantsRequest request,
			CancellationToken cancellationToken)
		{
			var result = await subscriptionService.GetTenantsByPlanAsync(
				id,
				request,
				cancellationToken);

			if (result.IsSuccess)
			{
				return Ok(result.Value);
			}

			return result.Failure switch
			{
				GetSubscriptionPlanTenantsFailure.PlanNotFound =>
					NotFound(CreateProblem(
						StatusCodes.Status404NotFound,
						"subscription_plan_not_found",
						"Không tìm thấy gói dịch vụ.")),

					_ => Problem(
						statusCode: StatusCodes.Status500InternalServerError,
						title: "get_subscription_plan_tenants_failed",
						detail: "Không thể tải danh sách tenant đang sử dụng gói.")
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
