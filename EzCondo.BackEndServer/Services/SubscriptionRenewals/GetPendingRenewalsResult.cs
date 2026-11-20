using EzCondo.BackEndServer.Contracts.SubscriptionRenewals;

namespace EzCondo.BackEndServer.Services.SubscriptionRenewals
{
	public sealed record GetPendingRenewalsResult(
		PendingRenewalsResponse? Value,
		GetPendingRenewalsFailure Failure)
	{
		public bool IsSuccess =>
			Failure == GetPendingRenewalsFailure.None &&
			Value is not null;

		public static GetPendingRenewalsResult Success(
			PendingRenewalsResponse value)
		{
			ArgumentNullException.ThrowIfNull(value);

			return new(
				value,
				GetPendingRenewalsFailure.None);
		}

		public static GetPendingRenewalsResult Failed(
			GetPendingRenewalsFailure failure)
		{
			if(failure == GetPendingRenewalsFailure.None)
			{
				throw new ArgumentException(
					"Kết quả thất bại phải có nguyên nhân.",
					nameof(failure));
			}

			return new(null, failure);
		}
	}
}
