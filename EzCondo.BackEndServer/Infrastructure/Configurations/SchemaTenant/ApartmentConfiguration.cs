using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaTenant
{
	internal sealed class ApartmentConfiguration()
		: EntityConfigurationBase<Apartment>("apartments", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<Apartment> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);
			ConfigureIsDeleted(b);
			ConfigureDeletedAt(b);

			b.Property(x => x.AreaM2)
				.HasPrecision(10, 2);

			b.Property(x => x.ApartmentNo)
				.IsRequired();

			b.Property(x => x.Bedroom)
				.HasColumnType("smallint");

			b.Property(x => x.TenantId)
				.IsRequired();
			
			b.HasOne(x => x.Tenant)
				.WithMany(x => x.Apartments)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.Resident)
				.WithMany(x => x.Apartments)
				.HasForeignKey(x => x.ApartmentRenter)
				.OnDelete(DeleteBehavior.NoAction);
			
			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"CK_tenant_apartments_areaM2_positive",
					"area_m2 > 0"
					);

				x.HasCheckConstraint(
					"CK_tenant_apartments_bedroom_positive",
					"bedroom >= 0"
					);
			});

			b.HasIndex(x => new { x.TenantId, x.ApartmentNo })
				.IsUnique();

			b.HasAlternateKey(x => new { x.TenantId, x.Id });
		}
	}
}
