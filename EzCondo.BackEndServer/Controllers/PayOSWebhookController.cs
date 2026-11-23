using EzCondo.BackEndServer.Services.Payments.PayOS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EzCondo.BackEndServer.Controllers
{
	[AllowAnonymous]
	[ApiController]
	[Route("api/payment/webhooks/payos")]
	public sealed class PayOSWebhookController(
		IPayOSWebhookProcessingService processingService)
		: ControllerBase
	{
		[HttpPost("{webhookKey:guid}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> Receive(
			Guid webhookKey,
			CancellationToken cancellationToken)
		{
			using var reader = new StreamReader(Request.Body);

			var rawPayload = await reader.ReadToEndAsync(cancellationToken);

			var result = await processingService.ProcessAsync(
				webhookKey,
				rawPayload,
				cancellationToken);

			return result.Status switch
			{
				PayOSWebhookProcessingStatus.Succeeded =>
					Ok(new
					{
						code = "00",
						desc = "success"
					}),

				PayOSWebhookProcessingStatus.Duplicate =>
					Ok(new
					{
						code = "00",
						desc = "already_processed"
					}),

				PayOSWebhookProcessingStatus.Unresolved =>
					Ok(new
					{
						code = "00",
						desc = "received_for_reconciliation"
					}),

				PayOSWebhookProcessingStatus.InvalidSignature =>
					BadRequest(new
					{
						code = "01",
						desc = result.Detail
					}),

				PayOSWebhookProcessingStatus.InvalidPayload =>
					BadRequest(new
					{
						code = "02",
						desc = result.Detail
					}),

				PayOSWebhookProcessingStatus.ProviderAccountNotFound =>
					NotFound(new
					{
						code = "03",
						desc = result.Detail
					}),

				_=>
					StatusCode(
						StatusCodes.Status500InternalServerError,
						new
						{
							code = "99",
							desc = "temporary_failure"
						})
			};
		}
	}
}
