namespace EzCondo.BackEndServer.Infrastructure.Payments.PayOS
{
	public sealed class PayOSGatewayException : Exception
	{
		public PayOSGatewayException(string message)
			: base(message)
		{
		}

		public PayOSGatewayException(
			string message,
			Exception innerException)
			: base(message, innerException)
		{
		}
	}
}