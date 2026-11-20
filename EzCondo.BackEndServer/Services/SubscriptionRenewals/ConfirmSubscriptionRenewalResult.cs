using EzCondo.BackEndServer.Contracts.SubscriptionRenewals;

namespace EzCondo.BackEndServer.Services.SubscriptionRenewals
{
	public sealed record ConfirmSubscriptionRenewalResult(
		ConfirmSubscriptionRenewalResponse? Value,
		ConfirmSubscriptionRenewalFailure Failure)
	{
		public bool IsSuccess =>
			Failure == ConfirmSubscriptionRenewalFailure.None &&
			Value is not null;

		public static ConfirmSubscriptionRenewalResult Success(
			ConfirmSubscriptionRenewalResponse value)
		{
			ArgumentNullException.ThrowIfNull(value);

			return new(
				value,
				ConfirmSubscriptionRenewalFailure.None);
		}

		public static ConfirmSubscriptionRenewalResult Failed(
			ConfirmSubscriptionRenewalFailure failure)
		{
			if(failure == ConfirmSubscriptionRenewalFailure.None)
			{
				throw new ArgumentException(
					"Kết quả thất bại phải có nguyên nhân.",
					nameof(failure));
			}

			return new(null, failure);
		}
	}
}
