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
		ITenantSubscriptionInvoiceService invoiceService,
		ITenantSubscriptionPaymentLinkService paymentService)
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

		[HttpPost("{invoiceId:guid}/payment-link")]
		[ProducesResponseType<SubscriptionPaymentLinkResponse>(StatusCodes.Status200OK)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<SubscriptionPaymentLinkResponse>> CreatePaymentLink(
			Guid invoiceId,
			CancellationToken cancellationToken)
		{
			var result = await paymentService.CreateAsync(
				invoiceId,
				cancellationToken);

			if (result.IsSuccess)
			{
				return Ok(result.Value);
			}

			return result.Failure switch
			{
				CreateSubscriptionPaymentLinkFailure.InvalidIdentity =>
					Unauthorized(CreateProblem(
						StatusCodes.Status401Unauthorized,
						"invalid_tenant_identity",
						"Không xác định được tài khoản hoặc tenant hiện tại.")),

				CreateSubscriptionPaymentLinkFailure.TenantAdminRequired =>
					StatusCode(
						StatusCodes.Status403Forbidden,
						CreateProblem(
							StatusCodes.Status403Forbidden,
							"tenant_admin_required",
							"Chỉ Chủ chung cư được thanh toán hóa đơn subscription.")),

				CreateSubscriptionPaymentLinkFailure.InvoiceNotFound =>
					NotFound(CreateProblem(
						StatusCodes.Status404NotFound,
						"subscription_invoice_not_found",
						"Không tìm thấy hóa đơn trong tenant hiện tại.")),

				CreateSubscriptionPaymentLinkFailure.InvoiceNotPayable =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"subscription_invoice_not_payable",
						"Hóa đơn không ở trạng thái có thể thanh toán hoặc đã được thanh toán đủ.")),

				CreateSubscriptionPaymentLinkFailure.RenewalRequestNotReady =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"renewal_request_not_ready_for_payment",
						"Yêu cầu gia hạn chưa ở trạng thái chờ thanh toán.")),

				CreateSubscriptionPaymentLinkFailure.ProviderAccountUnavailable =>
					StatusCode(
						StatusCodes.Status503ServiceUnavailable,
						CreateProblem(
							StatusCodes.Status503ServiceUnavailable,
							"payos_provider_account_unavailable",
							"Kênh thu tiền PayOS của Platform chưa sẵn sàng.")),


				CreateSubscriptionPaymentLinkFailure.PaymentLinkCreationInProgress =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"payment_link_creation_in_progress",
						"Payment link đang được tạo. Vui lòng thử lại sau ít phút.")),

				CreateSubscriptionPaymentLinkFailure.PaymentReconciliationPending =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"payment_reconciliation_pending",
						"PayOS đang xử lý hoặc đã nhận tiền. Hệ thống đang chờ webhook đối soát.")),

				CreateSubscriptionPaymentLinkFailure.PaymentProviderUnavailable =>
					StatusCode(
						StatusCodes.Status503ServiceUnavailable,
						CreateProblem(
							StatusCodes.Status503ServiceUnavailable,
							"payment_provider_unavailable",
							"Hiện không thể kết nối PayOS. Vui lòng thử lại sau.")),

				_ => Problem(
					statusCode:
						StatusCodes.Status500InternalServerError,
					title: "create_subscription_payment_link_failed",
					detail: "Không thể tạo payment link cho hóa đơn.")
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
