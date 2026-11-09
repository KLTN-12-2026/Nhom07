namespace EzCondo.BackEndServer.Services.Tenants
{
	public interface ITenantIdentityGenerator
	{
		TenantIdentity Generate(
			Guid tenantId,
			string legalName); //Sinh trường code và Slug
	}
}
