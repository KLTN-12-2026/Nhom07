namespace EzCondo.BackEndServer.Services.Payments.PayOS
{
	public enum PayOSWebhookProcessingStatus
	{
		Succeeded,
		Duplicate,
		InvalidSignature,
		InvalidPayload,
		ProviderAccountNotFound,
		Unresolved,
		TransientFailure
	}
}
