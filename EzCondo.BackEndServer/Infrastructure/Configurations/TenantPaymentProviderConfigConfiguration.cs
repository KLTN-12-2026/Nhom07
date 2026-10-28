using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class TenantPaymentProviderConfigConfiguration()
		: EntityConfigurationBase<TenantPaymentProviderConfig>("tenant_payment_provider_config", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<TenantPaymentProviderConfig> b)
		{
			ConfigureUuidPrimaryKey(b);

			b.Property(x => x.MerchantCode)
				.HasMaxLength(150);

			b.Property(x => x.SecretReference)
				.HasColumnType("text")
				.IsRequired();

			b.Property(x => x.IsActive)
				.HasDefaultValue(true)
				.IsRequired();

			b.Property(x => x.Settings)
				.HasColumnType("jsonb")
				.HasDefaultValueSql("'{}'::jsonb")
				.IsRequired();

			b.Property(x => x.TenantId)
				.IsRequired();

			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.PaymentProviderConfigs)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasAlternateKey(x => new { x.TenantId, x.Id });
		}
	}
}
