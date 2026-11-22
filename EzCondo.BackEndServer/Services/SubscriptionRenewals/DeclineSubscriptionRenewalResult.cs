using EzCondo.BackEndServer.Contracts.SubscriptionRenewals;

namespace EzCondo.BackEndServer.Services.SubscriptionRenewals
{
	public sealed record DeclineSubscriptionRenewalResult(
		DeclineSubscriptionRenewalResponse? Value,
		DeclineSubscriptionRenewalFailure Failure)
	{
		public bool IsSuccess =>
			Failure == DeclineSubscriptionRenewalFailure.None &&
			Value is not null;

		public static DeclineSubscriptionRenewalResult Success(
			DeclineSubscriptionRenewalResponse value)
		{
			return new(
				value,
				DeclineSubscriptionRenewalFailure.None);
		}

		public static DeclineSubscriptionRenewalResult Failed(
			DeclineSubscriptionRenewalFailure failure)
		{
			if(failure == DeclineSubscriptionRenewalFailure.None)
			{
				throw new ArgumentException(
					"Failure không được là None.",
					nameof(failure));
			}

			return new(null, failure);
		}
	}
}
