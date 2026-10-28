using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class ApartmentResidentConfiguration()
		: EntityConfigurationBase<ApartmentResident>("apartment_residents", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<ApartmentResident> b)
		{
			ConfigureUuidPrimaryKey(b);
			b.Property(x => x.OccupancyPeriod).HasColumnType("daterange").IsRequired();
			ConfigureCreatedAt(b);
			b.Property(x => x.TenantId).IsRequired();
			b.Property(x => x.ApartmentId).IsRequired();
			b.Property(x => x.ResidentId).IsRequired();

			b.HasOne(x => x.Tenant).WithMany(t => t.ApartmentResidents).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.Apartment)
				.WithMany(a => a.ApartmentResidents)
				.HasForeignKey(x => new {x.TenantId, x.ApartmentId })
				.HasPrincipalKey(x => new {x.TenantId, x.Id})
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.Resident)
				.WithMany(x => x.ApartmentResidents)
				.HasForeignKey(x => new {x.TenantId, x.ResidentId })
				.HasPrincipalKey(x => new {x.TenantId, x.Id})
				.OnDelete(DeleteBehavior.Cascade);

			b.ToTable(t => t.HasCheckConstraint("apartment_residents_period_check", "NOT isempty(occupancy_period)"));

			b.HasAlternateKey(x => new { x.TenantId, x.Id });
		}
	}
}
