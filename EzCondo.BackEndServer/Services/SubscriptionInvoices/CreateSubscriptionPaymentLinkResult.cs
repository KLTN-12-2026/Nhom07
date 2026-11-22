using EzCondo.BackEndServer.Contracts.SubscriptionInvoices;

namespace EzCondo.BackEndServer.Services.SubscriptionInvoices
{
	public sealed record CreateSubscriptionPaymentLinkResult(
		SubscriptionPaymentLinkResponse? Value,
		CreateSubscriptionPaymentLinkFailure Failure)
	{
		public bool IsSuccess =>
			Failure == CreateSubscriptionPaymentLinkFailure.None &&
			Value is not null;

		public static CreateSubscriptionPaymentLinkResult Success(
			SubscriptionPaymentLinkResponse value)
		{
			ArgumentNullException.ThrowIfNull(value);

			return new(
				value,
				CreateSubscriptionPaymentLinkFailure.None);
		}

		public static CreateSubscriptionPaymentLinkResult Failed(
			CreateSubscriptionPaymentLinkFailure failure)
		{
			if(failure == CreateSubscriptionPaymentLinkFailure.None)
			{
				throw new ArgumentException(
					"Kết quả thất bại phải có nguyên nhân.",
					nameof(failure));
			}

			return new(
				null,
				failure);
		}
	}
}
