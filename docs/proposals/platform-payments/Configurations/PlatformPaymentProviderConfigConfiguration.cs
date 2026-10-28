using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations;

internal sealed class PlatformPaymentProviderConfigConfiguration()
    : EntityConfigurationBase<PlatformPaymentProviderConfig>("payment_provider_configs", "platform")
{
    protected override void ConfigureEntity(EntityTypeBuilder<PlatformPaymentProviderConfig> b)
    {
        ConfigureUuidPrimaryKey(b);
        ConfigureCreatedAt(b);
        ConfigureUpdatedAt(b);
        b.Property(x => x.Code).HasMaxLength(80).IsRequired();
        b.Property(x => x.Provider).HasMaxLength(40).IsRequired();
        b.Property(x => x.MerchantCode).HasMaxLength(150);
        b.Property(x => x.SecretReference).HasColumnType("text").IsRequired();
        b.Property(x => x.Settings).HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'::jsonb").IsRequired();
        // CLR initializer đặt true; không có DB default để insert false vẫn được giữ đúng.
        b.Property(x => x.IsActive).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();
        b.ToTable(t =>
        {
            t.HasCheckConstraint("CK_platform_provider_code", "btrim(code) <> ''");
            t.HasCheckConstraint("CK_platform_provider_name", "btrim(provider) <> ''");
            t.HasCheckConstraint("CK_platform_provider_secret", "btrim(secret_reference) <> ''");
        });
    }
}
