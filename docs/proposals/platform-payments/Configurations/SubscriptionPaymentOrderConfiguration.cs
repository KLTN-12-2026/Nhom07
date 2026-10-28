using EzCondo.BackEndServer.Data.Entities;
using EzCondo.BackEndServer.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations;

internal sealed class SubscriptionPaymentOrderConfiguration()
    : EntityConfigurationBase<SubscriptionPaymentOrder>("subscription_payment_orders", "platform")
{
    protected override void ConfigureEntity(EntityTypeBuilder<SubscriptionPaymentOrder> b)
    {
        ConfigureUuidPrimaryKey(b);
        ConfigureCreatedAt(b);
        ConfigureUpdatedAt(b);
        // Đăng ký sequence bằng ModelBuilder.HasSequence<long> như README.
        b.Property(x => x.OrderCode)
            .HasDefaultValueSql("nextval('platform.subscription_payment_order_code_seq')")
            .ValueGeneratedOnAdd().IsRequired();
        b.Property(x => x.ProviderPaymentLinkId).HasMaxLength(255);
        // Dùng CLR enum hiện có, lưu varchar riêng; không phụ thuộc enum DB schema tenant.
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20)
            .HasDefaultValue(PaymentOrderStatus.Pending).IsRequired();
        b.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        b.Property(x => x.AmountPaid).HasPrecision(18, 2).HasDefaultValue(0m).IsRequired();
        b.Property(x => x.Currency).HasColumnType("character(3)")
            .HasDefaultValue("VND").IsRequired();
        b.Property(x => x.CheckoutUrl).HasColumnType("text");
        b.Property(x => x.QrCode).HasColumnType("text");
        b.Property(x => x.ReturnUrl).HasColumnType("text");
        b.Property(x => x.CancelUrl).HasColumnType("text");
        b.Property(x => x.CancellationReason).HasColumnType("text");
        b.Property(x => x.ExpiresAt).HasColumnType("timestamptz");
        b.Property(x => x.CancelledAt).HasColumnType("timestamptz");

        b.HasOne(x => x.SubscriptionInvoice).WithMany()
            .HasForeignKey(x => x.SubscriptionInvoiceId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.ProviderConfig).WithMany(x => x.PaymentOrders)
            .HasForeignKey(x => x.ProviderConfigId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.RequestedByUser).WithMany()
            .HasForeignKey(x => x.RequestedByUserId).OnDelete(DeleteBehavior.NoAction);

        b.HasAlternateKey(x => new { x.ProviderConfigId, x.Id });
        b.HasIndex(x => x.OrderCode).IsUnique();
        b.HasIndex(x => new { x.ProviderConfigId, x.ProviderPaymentLinkId })
            .IsUnique().HasFilter("provider_payment_link_id IS NOT NULL");
        // Không unique InvoiceId: cho phép retry bằng order mới.
        b.HasIndex(x => new { x.SubscriptionInvoiceId, x.CreatedAt });
        b.HasIndex(x => new { x.Status, x.ExpiresAt })
            .HasFilter("status IN ('Pending', 'Underpaid')");
        b.ToTable(t =>
        {
            t.HasCheckConstraint("CK_subscription_order_code", "order_code > 0");
            t.HasCheckConstraint("CK_subscription_order_amount", "amount > 0");
            t.HasCheckConstraint("CK_subscription_order_paid", "amount_paid >= 0");
            // Đề xuất ban đầu: Subscription thanh toán VND; xem README nếu đa tiền tệ.
            t.HasCheckConstraint("CK_subscription_order_currency", "currency = 'VND'");
            t.HasCheckConstraint("CK_subscription_order_status",
                "status IN ('Pending', 'Underpaid', 'Paid', 'Cancelled', 'Expired', 'Failed')");
            t.HasCheckConstraint("CK_subscription_order_link",
                "provider_payment_link_id IS NULL OR btrim(provider_payment_link_id) <> ''");
        });
    }
}
