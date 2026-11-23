using EzCondo.BackEndServer.Contracts.SubscriptionInvoices;

namespace EzCondo.BackEndServer.Services.SubscriptionInvoices
{
	public sealed record IssueSubscriptionInvoiceResult(
		IssueSubscriptionInvoiceResponse? Value,
		IssueSubscriptionInvoiceFailure Failure)
	{
		public bool IsSuccess =>
			Failure == IssueSubscriptionInvoiceFailure.None &&
			Value is not null;

		public static IssueSubscriptionInvoiceResult Success(
			IssueSubscriptionInvoiceResponse value)
		{
			ArgumentNullException.ThrowIfNull(value);

			return new(
				value,
				IssueSubscriptionInvoiceFailure.None);
		}

		public static IssueSubscriptionInvoiceResult Failed(
			IssueSubscriptionInvoiceFailure failure)
		{
			if(failure == IssueSubscriptionInvoiceFailure.None)
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
