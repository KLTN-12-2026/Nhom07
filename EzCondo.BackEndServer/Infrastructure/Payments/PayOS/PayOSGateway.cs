using EzCondo.BackEndServer.Infrastructure.Payments.PayOS.Models;
using EzCondo.BackEndServer.Infrastructure.Payments.PayOS.Options;
using Microsoft.Extensions.Options;
using PayOS.Exceptions;

using SdkRequestOptions =
	global::PayOS.Models.RequestOptions<
		global::PayOS.Models.V2.PaymentRequests.CreatePaymentLinkRequest>;

using SdkCreatePaymentLinkRequest =
	global::PayOS.Models.V2.PaymentRequests.CreatePaymentLinkRequest;

using SdkPayOSClient =
	global::PayOS.PayOSClient;

using SdkPayOSException =
	global::PayOS.Exceptions.PayOSException;

using SdkCreateRequestOptions =
	global::PayOS.Models.RequestOptions<
		global::PayOS.Models.V2.PaymentRequests.CreatePaymentLinkRequest>;

using SdkCancelRequestOptions =
	global::PayOS.Models.RequestOptions<
		global::PayOS.Models.V2.PaymentRequests.CancelPaymentLinkRequest>;

using SdkPaymentLink =
	global::PayOS.Models.V2.PaymentRequests.PaymentLink;

using SdkNotFoundException =
	global::PayOS.Exceptions.NotFoundException;

namespace EzCondo.BackEndServer.Infrastructure.Payments.PayOS
{
	public sealed class PayOSGateway(
		SdkPayOSClient client,
		IOptions<PayOSIntegrationOptions> options,
		ILogger<PayOSGateway> logger)
		: IPayOSGateway
	{
		private readonly PayOSIntegrationOptions _options = options.Value;

		public async Task<PayOSPaymentLinkResult> CreatePaymentLinkAsync(
			CreatePayOSPaymentLinkCommand command, 
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(command);

			if(command.OrderCode <= 0)
			{
				throw new ArgumentOutOfRangeException(
					nameof(command.OrderCode),
					"OrderCode phải lớn hơn 0.");
			}

			if(command.Amount <= 0)
			{
				throw new ArgumentOutOfRangeException(
					nameof(command.Amount),
					"Số tiền thanh toán phải lớn hơn 0.");
			}

			if(string.IsNullOrWhiteSpace(command.Description)){
				throw new ArgumentException(
					"Mô tả thanh toán không được để trống.",
					nameof(command.Description));
			}

			var request =
				new SdkCreatePaymentLinkRequest
				{
					OrderCode = command.OrderCode,
					Amount = command.Amount,
					Description = command.Description,
					ReturnUrl = _options.ReturnUrl,
					CancelUrl = _options.CancelUrl,
					ExpiredAt = command.ExpiresAt.ToUnixTimeSeconds()
				};

			try
			{
				var requestOptions = new SdkCreateRequestOptions
				{
					CancellationToken = cancellationToken
				};

				var response = await client.PaymentRequests.CreateAsync(
					request,
					requestOptions);

				ValidateResponse(command, response);

				var expiresAt = response.ExpiredAt.HasValue
					? DateTimeOffset.FromUnixTimeSeconds(response.ExpiredAt.Value)
					: command.ExpiresAt;

				return new PayOSPaymentLinkResult(
					response.OrderCode,
					response.Amount,
					response.Currency,
					response.PaymentLinkId,
					response.CheckoutUrl,
					response.QrCode,
					response.Status.ToString().ToLowerInvariant(),
					expiresAt);
			}
			catch (OperationCanceledException)
				when (cancellationToken.IsCancellationRequested)
			{
				throw;
			}
			catch (SdkPayOSException exception)
			{
				logger.LogError(
					exception,
					"PayOS từ chối hoặc không thể tạo payment link. " +
						"OrderCode: {OrderCode}",
					command.OrderCode);

				throw new PayOSGatewayException(
					"Không thể tạo payment link qua PayOS.",
					exception);
			}
		}

		public async Task<PayOSPaymentLinkStatusResult?> GetPaymentLinkStatusAsync(
			long orderCode,
			CancellationToken cancellationToken = default)
		{
			if(orderCode <= 0)
			{
				throw new ArgumentOutOfRangeException(
					nameof(orderCode),
					"OrderCode phải lớn hơn 0.");
			}

			try
			{
				var requestOptions = new SdkRequestOptions
				{
					CancellationToken = cancellationToken
				};

				var response = await client.PaymentRequests.GetAsync(
					orderCode,
					requestOptions);

				return MapPaymentLink(response, orderCode);
			}
			catch(SdkNotFoundException)
			{
				return null;
			}
			catch (SdkPayOSException exception)
			{
				logger.LogError(
					exception,
					"Không thể truy vấn PayOS payment link. OrderCode: {OrderCode}",
					orderCode);

				throw new PayOSGatewayException(
					"Không thể truy vấn payment link từ PayOS.",
					exception);
			}
		}

		public async Task<PayOSPaymentLinkStatusResult> CancelPaymentLinkAsync(
			long orderCode,
			string cancellationReason,
			CancellationToken cancellationToken = default)
		{
			if(orderCode <= 0)
			{
				throw new ArgumentOutOfRangeException(
					nameof(orderCode),
					"OrderCode phải lớn hơn 0.");
			}

			if (string.IsNullOrWhiteSpace(cancellationReason))
			{
				throw new ArgumentException(
					"Lý do hủy payment link không được để trống.",
					nameof(cancellationReason));
			}

			try
			{
				var requestOptions = new SdkCancelRequestOptions
				{
					CancellationToken = cancellationToken
				};

				var response = await client.PaymentRequests.CancelAsync(
					orderCode,
					cancellationReason.Trim(),
					requestOptions);

				return MapPaymentLink(response, orderCode);
			}
			catch(OperationCanceledException)
				when (cancellationToken.IsCancellationRequested)
			{
				throw;
			}
			catch(SdkPayOSException exception)
			{
				logger.LogError(
					exception,
					"Không thể hủy PayOS payment link. OrderCode: {OrderCode}",
					orderCode);

				throw new PayOSGatewayException(
					"Không thể hủy payment link trên PayOS",
					exception);
			}
		}

		public async Task<PayOSVerifiedWebhookData> VerifyWebhookAsync(
			string rawPayload, 
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(rawPayload))
			{
				throw new PayOSWebhookVerificationException(
					"Payload webhook PayOS không được để trống.");
			}

			cancellationToken.ThrowIfCancellationRequested();

			try
			{
				var data = await client.Webhooks.VerifyAsync(rawPayload);

				if (data is null)
				{
					throw new PayOSWebhookVerificationException(
						"PayOS webhook không có dữ liệu giao dịch.");
				}

				return new PayOSVerifiedWebhookData(
					data.OrderCode,
					data.Amount,
					data.Description,
					data.AccountNumber,
					data.Reference,
					data.TransactionDateTime,
					data.Currency,
					data.PaymentLinkId,
					data.Code,
					data.Description2,
					data.CounterAccountBankId ?? string.Empty,
					data.CounterAccountBankName ?? string.Empty,
					data.CounterAccountName ?? string.Empty,
					data.CounterAccountNumber ?? string.Empty,
					data.VirtualAccountName ?? string.Empty,
					data.VirtualAccountNumber ?? string.Empty);
			}
			catch(WebhookException exception)
			{
				logger.LogWarning(
					exception,
					"PayOS webhook không hợp lệ.");

				throw new PayOSWebhookVerificationException(
					"Không thể xác minh chữ ký webhook PayOS.",
					exception);
			}
		}

		private static void ValidateResponse(
			CreatePayOSPaymentLinkCommand command,
			global::PayOS.Models.V2.PaymentRequests
				.CreatePaymentLinkResponse response)
		{
			ArgumentNullException.ThrowIfNull(response);

			if(response.OrderCode != command.OrderCode)
			{
				throw new PayOSGatewayException("PayOS trả về OrderCode không khớp.");
			}

			if(response.Amount != command.Amount)
			{
				throw new PayOSGatewayException("PayOS trả về số tiền không khớp.");
			}

			if(string.IsNullOrWhiteSpace(response.PaymentLinkId) ||
				string.IsNullOrWhiteSpace(response.CheckoutUrl) ||
				string.IsNullOrWhiteSpace(response.QrCode) ||
				string.IsNullOrWhiteSpace(response.Currency))
			{
				throw new PayOSGatewayException("PayOS trả về payment link không đầy đủ.");
			}

			if (!string.Equals(
					response.Currency,
					"VND",
					StringComparison.OrdinalIgnoreCase))
			{
				throw new PayOSGatewayException(
					"PayOS trả về loại tiền không được hệ thống hỗ trợ.");
			}
		}

		private static PayOSPaymentLinkStatusResult MapPaymentLink(
			SdkPaymentLink response, 
			long expectedOrderCode)
		{
			ArgumentNullException.ThrowIfNull(response);

			if(response.OrderCode != expectedOrderCode)
			{
				throw new PayOSGatewayException(
					"PayOS trả về OrderCode không khớp.");
			}

			if (string.IsNullOrWhiteSpace(response.Id))
			{
				throw new PayOSGatewayException(
					"PayOS trả về payment link không có Id.");
			}

			return new PayOSPaymentLinkStatusResult(
				response.OrderCode,
				response.Id,
				response.Amount,
				response.AmountPaid,
				response.AmountRemaining,
				response.Status.ToString().ToLowerInvariant());
		}
	}
}
