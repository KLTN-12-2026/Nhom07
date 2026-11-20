using EzCondo.BackEndServer.Contracts.SubscriptionInvoices;
using EzCondo.BackEndServer.Infrastructure.Authentication;
using EzCondo.BackEndServer.Services.SubscriptionInvoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EzCondo.BackEndServer.Controllers
{
	[ApiController]
	[Authorize(Policy = AuthPolicies.PlatformScope)]
	[Route("api/platform/subscription-invoices")]
	public class PlatformSubscriptionInvoicesController(
		ISubscriptionInvoiceService invoiceService)
		: ControllerBase
	{
		[HttpPost("{invoiceId:guid}/issue")]
		[ProducesResponseType<IssueSubscriptionInvoiceResponse>(StatusCodes.Status200OK)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<IssueSubscriptionInvoiceResponse>> Issue(
			[FromRoute] Guid invoiceId,
			CancellationToken cancellationToken)
		{
			var result = await invoiceService.IssueAsync(
				invoiceId,
				cancellationToken);

			if (result.IsSuccess)
			{
				return Ok(result.Value);
			}

			return result.Failure switch
			{
				IssueSubscriptionInvoiceFailure.InvalidIdentity =>
					Unauthorized(CreateProblem(
						StatusCodes.Status401Unauthorized,
						"invalid_platform_identity",
						"Không xác định được tài khoản Platform hiện tại.")),

				IssueSubscriptionInvoiceFailure.InvoiceNotFound =>
					NotFound(CreateProblem(
						StatusCodes.Status404NotFound,
						"subscription_invoice_not_found",
						"Không tìm thấy hóa đơn subscription.")),

				IssueSubscriptionInvoiceFailure.InvoiceNotDraft =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"subscription_invoice_not_draft",
						"Chỉ hóa đơn nháp mới được phát hành.")),

				IssueSubscriptionInvoiceFailure.RenewalRequestNotFound =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"invoice_renewal_link_missing",
						"Hóa đơn không liên kết với yêu cầu gia hạn hợp lệ.")),

				IssueSubscriptionInvoiceFailure.RenewalRequestNotReady =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"renewal_request_not_ready_for_issue",
						"Yêu cầu gia hạn không ở trạng thái chờ phát hành hóa đơn.")),

				IssueSubscriptionInvoiceFailure.RenewalRequestStale =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"renewal_request_stale",
						"Tenant hoặc subscription đã thay đổi so với yêu cầu gia hạn.")),

				IssueSubscriptionInvoiceFailure.InvoiceSnapshotMismatch =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"invoice_snapshot_mismatch",
						"Dữ liệu hóa đơn không khớp với nội dung gia hạn đã được xác nhận.")),

				IssueSubscriptionInvoiceFailure.InvoiceDueDateElapsed =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"invoice_due_date_elapsed",
						"Đã qua hạn thanh toán nên hóa đơn không thể được phát hành.")),

				IssueSubscriptionInvoiceFailure.PlatformAdminRequired =>
					StatusCode(
						StatusCodes.Status403Forbidden,
						CreateProblem(
							StatusCodes.Status403Forbidden,
							"platform_admin_required",
							"Chỉ Quản trị viên Platform được phát hành hóa đơn subscription.")),

				_ => Problem(
					statusCode: StatusCodes.Status500InternalServerError,
					title: "issue_subscription_invoice_failed",
					detail: "Không thể phát hành hóa đơn subscription.")
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
