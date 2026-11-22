using EzCondo.BackEndServer.Infrastructure.Payments.PayOS.Models;

namespace EzCondo.BackEndServer.Infrastructure.Payments.PayOS
{
	public interface IPayOSGateway
	{
		Task<PayOSPaymentLinkResult> CreatePaymentLinkAsync(
			CreatePayOSPaymentLinkCommand command,
			CancellationToken cancellationToken = default);

		Task<PayOSPaymentLinkStatusResult?> GetPaymentLinkStatusAsync(
			long orderCode,
			CancellationToken cancellationToken = default);

		Task<PayOSPaymentLinkStatusResult> CancelPaymentLinkAsync(
			long orderCode,
			string cancellationReason,
			CancellationToken cancellationToken = default);

		Task<PayOSVerifiedWebhookData> VerifyWebhookAsync(
			string rawPayload,
			CancellationToken cancellationToken = default);
	}
}
