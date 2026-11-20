using EzCondo.BackEndServer.Contracts.SubscriptionRenewals;
using EzCondo.BackEndServer.Infrastructure.Authentication;
using EzCondo.BackEndServer.Services.SubscriptionRenewals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EzCondo.BackEndServer.Contracts.SubscriptionInvoices;
using EzCondo.BackEndServer.Services.SubscriptionInvoices;

namespace EzCondo.BackEndServer.Controllers
{
	[ApiController]
	[Authorize(Policy = AuthPolicies.PlatformScope)]
	[Route("api/platform/subscription-renewals")]
	public sealed class PlatformSubscriptionRenewalsController(
		IPlatformSubscriptionRenewalService renewalService,
		ISubscriptionInvoiceService invoiceService)
		: ControllerBase
	{
		[HttpGet("pending-review")]
		[ProducesResponseType<PendingRenewalsResponse>(StatusCodes.Status200OK)]
		[ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<PendingRenewalsResponse>> GetPendingReview(
			[FromQuery] GetPendingRenewalsRequest request,
			CancellationToken cancellationToken)
		{
			var result = await renewalService.GetPendingReviewAsync(
				request,
				cancellationToken);

			if (result.IsSuccess)
			{
				return Ok(result.Value);
			}

			return result.Failure switch
			{
				GetPendingRenewalsFailure
					.InvalidIdentity =>
						Unauthorized(CreateProblem(
							StatusCodes.Status401Unauthorized,
							"invalid_platform_identity",
							"Không xác định được tài khoản Platform hiện tại.")),

				GetPendingRenewalsFailure
					.PlatformAdminRequired =>
						StatusCode(
							StatusCodes.Status403Forbidden,
							CreateProblem(
								StatusCodes.Status403Forbidden,
								"platform_admin_required",
								"Chỉ Quản trị viên Platform được xem hàng chờ gia hạn.")),

				GetPendingRenewalsFailure
					.InvalidPagination =>
						BadRequest(CreateProblem(
							StatusCodes.Status400BadRequest,
							"invalid_pagination",
							"PageNumber hoặc PageSize không hợp lệ.")),

				_ => Problem(
					statusCode: StatusCodes.Status500InternalServerError,
					title: "get_pending_subscription_renewals_failed",
					detail: "Không thể tải hàng chờ gia hạn.")
			};
		}

		[HttpPost("{requestId:guid}/invoice-draft")]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
		[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType<SubscriptionInvoiceResponse>(StatusCodes.Status201Created)]

		public async Task<ActionResult<SubscriptionInvoiceResponse>> CreateInvoiceDraft(
			Guid requestId,
			CancellationToken cancellationToken)
		{
			var result = await invoiceService.CreateDraftFromRenewalAsync(
				requestId,
				cancellationToken);

			if (result.IsSuccess)
			{
				return StatusCode(
					StatusCodes.Status201Created,
					result.Value);
			}

			return result.Failure switch
			{
				CreateSubscriptionInvoiceDraftFailure.InvalidIdentity =>
					Unauthorized(CreateProblem(
						StatusCodes.Status401Unauthorized,
						"invalid_platform_identity",
						"Không xác định được tài khoản Platform hiện tại.")),

				CreateSubscriptionInvoiceDraftFailure.PlatformAdminRequired =>
					StatusCode(
						StatusCodes.Status403Forbidden,
						CreateProblem(
							StatusCodes.Status403Forbidden,
							"platform_admin_required",
							"Chỉ Quản trị viên Platform được tạo hóa đơn subscription.")),

				CreateSubscriptionInvoiceDraftFailure.RenewalRequestNotFound =>
					NotFound(CreateProblem(
						StatusCodes.Status404NotFound,
						"renewal_request_not_found",
						"Không tìm thấy yêu cầu gia hạn.")),

				CreateSubscriptionInvoiceDraftFailure.RenewalRequestAlreadyReviewed =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"renewal_request_already_reviewed",
						"Yêu cầu gia hạn đã được Quản trị viên xử lý.")),

				CreateSubscriptionInvoiceDraftFailure.RenewalRequestStale =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"renewal_request_stale",
						"Subscription hoặc Tenant đã thay đổi so với yêu cầu gia hạn.")),

				CreateSubscriptionInvoiceDraftFailure.RenewalSnapshotInvalid =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"renewal_snapshot_invalid",
						"Thông tin gói hoặc kỳ thanh toán trong yêu cầu gia hạn không hợp lệ.")),

				CreateSubscriptionInvoiceDraftFailure.InvoiceAlreadyExistsForPeriod =>
					Conflict(CreateProblem(
						StatusCodes.Status409Conflict,
						"invoice_already_exists_for_period",
						"Đã tồn tại hóa đơn cho subscription và kỳ thanh toán này.")),

				_ => Problem(
					statusCode: StatusCodes.Status500InternalServerError,
					title: "create_subscription_invoice_draft_failed",
					detail: "Không thể tạo hóa đơn subscription")
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
