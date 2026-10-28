using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class TenantServiceConfiguration()
		: EntityConfigurationBase<TenantService>("tenant_services", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<TenantService> b)
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

			b.Property(x => x.BillingType)
				.HasColumnType("tenant.service_billing_type")
				.IsRequired();

			b.Property(x => x.Unit)
				.HasMaxLength(30)
				.IsRequired();

			b.Property(x => x.IsActive)
				.HasDefaultValue(true)
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.TenantServices)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasIndex(x => new { x.TenantId, x.Id }).IsUnique();
			b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();

			b.HasAlternateKey(x => new { x.TenantId, x.Id });
		}
	}
}
