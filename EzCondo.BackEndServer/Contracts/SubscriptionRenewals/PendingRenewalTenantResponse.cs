namespace EzCondo.BackEndServer.Contracts.SubscriptionRenewals
{
	public sealed record PendingRenewalTenantResponse(
		Guid TenantId,
		string TenantCode,
		string TenantSlug,
		string LegalName);
}
