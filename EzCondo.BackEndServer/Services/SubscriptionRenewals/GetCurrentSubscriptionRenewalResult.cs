using EzCondo.BackEndServer.Contracts.SubscriptionRenewals;

namespace EzCondo.BackEndServer.Services.SubscriptionRenewals
{
	public sealed record GetCurrentSubscriptionRenewalResult(
		CurrentSubscriptionRenewalResponse? Value,
		GetCurrentSubscriptionRenewalFailure Failure)
	{
		public bool IsSuccess =>
			Failure == GetCurrentSubscriptionRenewalFailure.None &&
			Value is not null;

		public static GetCurrentSubscriptionRenewalResult Success(
			CurrentSubscriptionRenewalResponse value)
		{
			ArgumentNullException.ThrowIfNull(value);

			return new(
				value,
				GetCurrentSubscriptionRenewalFailure.None);
		}

		public static GetCurrentSubscriptionRenewalResult Failed(
			GetCurrentSubscriptionRenewalFailure failure)
		{
			if(failure == GetCurrentSubscriptionRenewalFailure.None)
			{
				throw new ArgumentException(
					"Kết quả thất bại phải có nguyên nhân.",
					nameof(failure));
			}
			return new(null, failure);
		}
	}
}
