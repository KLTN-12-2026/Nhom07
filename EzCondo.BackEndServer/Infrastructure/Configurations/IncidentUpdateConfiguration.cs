using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class IncidentUpdateConfiguration()
		: EntityConfigurationBase<IncidentUpdate>("incident_updates", "platform")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<IncidentUpdate> b)
		{
			ConfigureUuidPrimaryKey(b);

			b.Property(x => x.Status)
				.HasColumnType("platform.system_incident_status")
				.IsRequired();

			b.Property(x => x.Message)
				.HasColumnType("text")
				.IsRequired();

			ConfigureCreatedAt(b);

			b.Property(x => x.IncidentId).IsRequired();
			b.Property(x => x.AuthorId).IsRequired();

			b.HasOne(x => x.SystemIncident)
				.WithMany(x => x.IncidentUpdates)
				.HasForeignKey(x => x.IncidentId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.User)
				.WithMany(x => x.IncidentUpdates)
				.HasForeignKey(x => x.AuthorId)
				.OnDelete(DeleteBehavior.NoAction);
		}
	}
}
