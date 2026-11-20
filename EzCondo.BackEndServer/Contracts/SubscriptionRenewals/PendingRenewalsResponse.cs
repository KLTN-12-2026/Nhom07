namespace EzCondo.BackEndServer.Contracts.SubscriptionRenewals
{
	public sealed record PendingRenewalsResponse
	(
		int PageNumber,
		int PageSize,
		int TotalCount,
		int TotalPages,
		IReadOnlyCollection<PendingRenewalItemResponse> Items);
}
