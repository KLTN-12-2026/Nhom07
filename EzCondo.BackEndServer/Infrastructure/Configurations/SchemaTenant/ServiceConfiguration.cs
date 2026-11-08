using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaTenant
{
	internal sealed class ServiceConfiguration()
		: EntityConfigurationBase<Service>("services", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<Service> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);
			ConfigureIsDeleted(b);
			ConfigureDeletedAt(b);

			b.Property(x => x.Code)
				.HasMaxLength(50)
				.IsRequired();

			b.Property(x => x.Name)
				.HasMaxLength(150)
				.IsRequired();

			b.Property(x => x.Price)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.Services)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.ManagerCreated)
				.WithMany(x => x.CreatedByManager)
				.HasForeignKey(x => new { x.TenantId, x.CreatedByManager })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.ManagerUpdated)
				.WithMany(x => x.UpdatedByManager)
				.HasForeignKey(x => new { x.TenantId, x.UpdatedByManager })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.ManagerDeleted)
				.WithMany(x => x.DeletedByManager)
				.HasForeignKey(x => new { x.TenantId, x.DeletedByManager })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();

			b.HasAlternateKey(x => new { x.TenantId, x.Id });
		}
	}
}
