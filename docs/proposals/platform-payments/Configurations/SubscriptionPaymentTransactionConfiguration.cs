using EzCondo.BackEndServer.Data.Entities;
using EzCondo.BackEndServer.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations;

internal sealed class SubscriptionPaymentTransactionConfiguration()
    : EntityConfigurationBase<SubscriptionPaymentTransaction>("subscription_payment_transactions", "platform")
{
    protected override void ConfigureEntity(EntityTypeBuilder<SubscriptionPaymentTransaction> b)
    {
        ConfigureUuidPrimaryKey(b);
        ConfigureCreatedAt(b);
        b.Property(x => x.ProviderTransactionId).HasMaxLength(255).IsRequired();
        b.Property(x => x.ProviderReference).HasMaxLength(255);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20)
            .HasDefaultValue(PaymentTransactionStatus.Received).IsRequired();
        b.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        b.Property(x => x.OccurredAt).HasColumnType("timestamptz").IsRequired();
        b.Property(x => x.PayerMasked).HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'::jsonb").IsRequired();
        b.Property(x => x.ProviderPayload).HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'::jsonb").IsRequired();

        b.HasOne(x => x.ProviderConfig).WithMany(x => x.PaymentTransactions)
            .HasForeignKey(x => x.ProviderConfigId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.PaymentOrder).WithMany(x => x.PaymentTransactions)
            .HasForeignKey(x => new { x.ProviderConfigId, x.PaymentOrderId })
            .HasPrincipalKey(x => new { x.ProviderConfigId, x.Id })
            .OnDelete(DeleteBehavior.NoAction);

        // Cùng giao dịch provider không thể gán vào hai order của cùng cấu hình.
        b.HasIndex(x => new { x.ProviderConfigId, x.ProviderTransactionId }).IsUnique();
        b.HasIndex(x => new { x.PaymentOrderId, x.OccurredAt });
        b.ToTable(t =>
        {
            t.HasCheckConstraint("CK_subscription_transaction_amount", "amount > 0");
            t.HasCheckConstraint("CK_subscription_transaction_provider_id",
                "btrim(provider_transaction_id) <> ''");
            t.HasCheckConstraint("CK_subscription_transaction_status",
                "status IN ('Received', 'Verified', 'Rejected', 'Refunded')");
        });
    }
}
