using EzCondo.BackEndServer.Contracts.SubscriptionInvoices;

namespace EzCondo.BackEndServer.Services.SubscriptionInvoices
{
	public sealed record CreateSubscriptionInvoiceDraftResult(
		SubscriptionInvoiceResponse? Value,
		CreateSubscriptionInvoiceDraftFailure Failure)
	{
		public bool IsSuccess =>
			Failure == CreateSubscriptionInvoiceDraftFailure.None &&
			Value is not null;

		public static CreateSubscriptionInvoiceDraftResult Success(
			SubscriptionInvoiceResponse value)
		{
			ArgumentNullException.ThrowIfNull(value);

			return new(
				value,
				CreateSubscriptionInvoiceDraftFailure.None);
		}

		public static CreateSubscriptionInvoiceDraftResult Failed(
			CreateSubscriptionInvoiceDraftFailure failure)
		{
			if(failure == CreateSubscriptionInvoiceDraftFailure.None)
			{
				throw new ArgumentException(
					"Kết quả thất bại phải có nguyên nhân.",
					nameof(failure));
			}

			return new(null, failure);
		}
	}
}
