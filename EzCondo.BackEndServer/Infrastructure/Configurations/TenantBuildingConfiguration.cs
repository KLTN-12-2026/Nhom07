using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class TenantBuildingConfiguration()
		: EntityConfigurationBase<TenantBuilding>("tenant_buildings", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<TenantBuilding> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);

			b.Property(x => x.Name)
				.HasMaxLength(150)
				.IsRequired();

			b.Property(x => x.Address)
				.HasColumnType("text")
				.IsRequired();

			b.Property(x => x.TenantId)
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.TenantBuildings)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasIndex(x => new { x.TenantId, x.Id })
				.IsUnique();

			b.HasAlternateKey(x => new { x.TenantId, x.Id });
		}
	}
}
