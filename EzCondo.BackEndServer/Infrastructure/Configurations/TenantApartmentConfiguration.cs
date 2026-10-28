using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class TenantApartmentConfiguration()
		: EntityConfigurationBase<TenantApartment>("tenant_apartments", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<TenantApartment> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);

			b.Property(x => x.AreaM2)
				.HasPrecision(10, 2);

			b.Property(x => x.ApartmentNo)
				.IsRequired();

			b.Property(x => x.Bedroom)
				.HasColumnType("smallint");

			b.Property(x => x.Status)
				.HasColumnType("tenant.apartment_status_enum")
				.HasDefaultValueSql("'available'::tenant.apartment_status_enum")
				.IsRequired();

			b.Property(x => x.TenantId)
				.IsRequired();

			b.Property(x => x.BuildingId)
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.TenantApartments)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.TenantBuilding)
				.WithMany(x => x.TenantApartments)
				.HasForeignKey(x => new {x.TenantId, x.BuildingId })
				.HasPrincipalKey(x => new {x.TenantId, x.Id})
				.OnDelete(DeleteBehavior.Cascade);

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

			b.HasIndex(x => new { x.TenantId, x.Id }).IsUnique();
			b.HasIndex(x => new { x.TenantId, x.ApartmentNo, x.BuildingId }).IsUnique();

			b.HasAlternateKey(x => new { x.TenantId, x.Id });
		}
	}
}
