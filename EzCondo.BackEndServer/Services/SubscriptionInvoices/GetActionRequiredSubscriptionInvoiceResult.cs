using EzCondo.BackEndServer.Contracts.SubscriptionInvoices;

namespace EzCondo.BackEndServer.Services.SubscriptionInvoices
{
	public sealed record GetActionRequiredSubscriptionInvoiceResult(
		ActionRequiredSubscriptionInvoicesResponse? Value,
		GetActionRequiredSubscriptionInvoicesFailure Failure)
	{
		public bool IsSuccess =>
			Failure == GetActionRequiredSubscriptionInvoicesFailure.None &&
			Value is not null;

		public static GetActionRequiredSubscriptionInvoiceResult Success(
			ActionRequiredSubscriptionInvoicesResponse value)
		{
			ArgumentNullException.ThrowIfNull(value);

			return new(
				value,
				GetActionRequiredSubscriptionInvoicesFailure.None);
		}

		public static GetActionRequiredSubscriptionInvoiceResult Failed(
			GetActionRequiredSubscriptionInvoicesFailure failure)
		{
			if(failure == GetActionRequiredSubscriptionInvoicesFailure.None)
			{
				throw new ArgumentException(
					"Kết quả thất bại phải có nguyên nhân.",
					nameof(failure));
			}

			return new(null, failure);
		}
	}
}
