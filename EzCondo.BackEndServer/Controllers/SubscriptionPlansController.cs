using EzCondo.BackEndServer.Contracts.SubscriptionPlans;
using EzCondo.BackEndServer.Services.SubscriptionPlans;
using Microsoft.AspNetCore.Mvc;

namespace EzCondo.BackEndServer.Controllers
{
	[ApiController]
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
	}
}
