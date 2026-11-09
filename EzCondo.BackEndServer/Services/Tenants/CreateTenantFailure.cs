namespace EzCondo.BackEndServer.Services.Tenants
{
	public enum CreateTenantFailure
	{
		None,
		SubscriptionPlanNotFound,
		SubscriptionPlanUnavailable,
		TaxCodeAlreadyExists,
		GeneratedidentityCollision
	}
}
