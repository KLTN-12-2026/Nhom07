namespace EzCondo.BackEndServer.Infrastructure.Payments.PayOS
{
	public sealed class PayOSWebhookVerificationException : Exception
	{
		public PayOSWebhookVerificationException(
			string message,
			Exception? innerException = null)
			: base(message, innerException)
		{
		}
	}
}
