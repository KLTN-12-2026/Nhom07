using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class SystemIncidentConfiguration()
		: EntityConfigurationBase<SystemIncident>("system_incidents", "platform")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<SystemIncident> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);
			ConfigureDeletedAt(b);
			ConfigureIsDeleted(b);

			b.Property(x => x.Code)
				.IsRequired();

			b.Property(x => x.Title)
				.HasMaxLength(150)
				.IsRequired();

			b.Property(x => x.Description)
				.HasColumnType("text");

			b.Property(x => x.Severity)
				.HasColumnType("smallint")
				.IsRequired();

			b.Property(x => x.Status)
				.HasColumnType("platform.system_incident_status")
				.HasDefaultValueSql("'investigating'::platform.system_incident_status")
				.IsRequired();

			b.Property(x => x.CreatedBy)
				.IsRequired();

			b.HasOne(x => x.User)
				.WithMany(x => x.SystemIncidents)
				.HasForeignKey(x => x.CreatedBy)
				.OnDelete(DeleteBehavior.NoAction);
		}
	}
}
