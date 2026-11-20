using EzCondo.BackEndServer.Contracts.SubscriptionInvoices;
using EzCondo.BackEndServer.Infrastructure.Authentication;
using EzCondo.BackEndServer.Services.SubscriptionInvoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EzCondo.BackEndServer.Controllers
{
	[ApiController]
	[Authorize(Policy=AuthPolicies.TenantScope)]
	[Route("api/tenant/subscription-invoices")]
	public sealed class TenantSubscriptionInvoicesController(
		ITenantSubscriptionInvoiceService invoiceService)
		: ControllerBase
	{
		[HttpGet("action-required")]
		[ProducesResponseType<ActionRequiredSubscriptionInvoicesResponse>
			(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<ActionRequiredSubscriptionInvoicesResponse>> GetActionRequired(
			CancellationToken cancellationToken)
		{
			var result = await invoiceService.GetActionRequiredAsync(
				cancellationToken);

			if (result.IsSuccess)
			{
				return Ok(result.Value);
			}

			return result.Failure switch
			{
				GetActionRequiredSubscriptionInvoicesFailure
					.InvalidIdentity =>
						Unauthorized(CreateProblem(
							StatusCodes.Status401Unauthorized,
							"invalid_tenant_identity",
							"Không xác định được tài khoản hoặc tenant hiện tại.")),

				GetActionRequiredSubscriptionInvoicesFailure
					.TenantAdminRequired =>
						StatusCode(
							StatusCodes.Status403Forbidden,
							CreateProblem(
								StatusCodes.Status403Forbidden,
								"tenant_admin_required",
								"Chỉ Chủ chung cư được xem hóa đơn subscription cần thanh toán.")),

				_ => Problem(
					statusCode: StatusCodes.Status500InternalServerError,
					title: "get_action_required_subscription_invoices_failed",
					detail: "Không thể tải danh sách hóa đơn cần thanh toán.")
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
