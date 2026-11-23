namespace EzCondo.BackEndServer.Contracts.SubscriptionInvoices
{
	public sealed record ActionRequiredSubscriptionInvoicesResponse
	(
		int TotalCount,
		IReadOnlyCollection<ActionRequiredSubscriptionInvoiceItemResponse> Items);
}
