namespace EzCondo.BackEndServer.Services.Payments.PayOS
{
	public interface IPayOSWebhookProcessingService
	{
		Task<PayOSWebhookProcessingResult> ProcessAsync(
			Guid webhookKey,
			string rawPayload,
			CancellationToken cancellationToken = default);
	}
}
