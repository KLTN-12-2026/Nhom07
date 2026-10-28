using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class IncidentAffectedTenantConfiguration()
		: EntityConfigurationBase<IncidentAffectedTenant>("incident_affected_tenants", "platform")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<IncidentAffectedTenant> b)
		{
			b.HasKey(x => new { x.IncidentId, x.TenantId });
			b.Property(x => x.NotifiedAt);
			b.Property(x => x.IncidentId).IsRequired();
			b.Property(x => x.TenantId).IsRequired();

			b.HasOne(x => x.SystemIncident)
				.WithMany(x => x.IncidentAffectedTenants)
				.HasForeignKey(x => x.IncidentId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.IncidentAffectedTenants)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
