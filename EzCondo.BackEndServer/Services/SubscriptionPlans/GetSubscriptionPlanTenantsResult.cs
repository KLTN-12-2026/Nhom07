using EzCondo.BackEndServer.Contracts.SubscriptionPlans;

namespace EzCondo.BackEndServer.Services.SubscriptionPlans
{
	public sealed record GetSubscriptionPlanTenantsResult(
		SubscriptionPlanTenantsResponse? Value,
		GetSubscriptionPlanTenantsFailure Failure)
	{
		public bool IsSuccess =>
			Failure == GetSubscriptionPlanTenantsFailure.None &&
			Value is not null;

		public static GetSubscriptionPlanTenantsResult Success(
			SubscriptionPlanTenantsResponse value)
		{
			ArgumentNullException.ThrowIfNull(value);

			return new(
				value,
				GetSubscriptionPlanTenantsFailure.None);
		}

		public static GetSubscriptionPlanTenantsResult Failed(
			GetSubscriptionPlanTenantsFailure failure)
		{
			if(failure == GetSubscriptionPlanTenantsFailure.None)
			{
				throw new ArgumentException(
					"Kết quả thất bại phải có nguyên nhân.",
					nameof(failure));
			}

			return new(null, failure);
		}
	}
}
