namespace EzCondo.BackEndServer.Services.Payments.PayOS
{
	public sealed record PayOSWebhookProcessingResult(
		PayOSWebhookProcessingStatus Status,
		string? Detail = null)
	{
		public bool IsAccepted =>
			Status is
				PayOSWebhookProcessingStatus.Succeeded or
				PayOSWebhookProcessingStatus.Duplicate or
				PayOSWebhookProcessingStatus.Unresolved;
	}
}
