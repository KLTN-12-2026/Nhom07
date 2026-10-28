namespace EzCondo.BackEndServer.Data.Entities
{
	public class IncidentAffectedTenant
	{
		public DateTimeOffset? NotifiedAt { get; set; }

		public Guid IncidentId { get; set; }
		public SystemIncident SystemIncident { get; set; }
		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }
	}
}
