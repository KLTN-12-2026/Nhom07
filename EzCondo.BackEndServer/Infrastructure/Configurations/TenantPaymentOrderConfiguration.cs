using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class TenantPaymentOrderConfiguration()
		: EntityConfigurationBase<TenantPaymentOrder>("tenant_payment_orders", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<TenantPaymentOrder> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);

			b.Property(x => x.OrderCode)
				.HasDefaultValueSql("nextval('tenant.payos_order_code_seq')")
				.ValueGeneratedOnAdd() //Tự động sinh khi add vào DB
				.IsRequired();

			b.Property(x => x.ProviderPaymentLinkId)
				.HasMaxLength(255);

			b.Property(x => x.Status)
				.HasColumnType("tenant.payment_order_status")
				.HasDefaultValueSql("'pending'::tenant.payment_order_status")
				.IsRequired();

			b.Property(x => x.Amount)
				.HasPrecision(18, 2)
				.IsRequired();

			b.Property(x => x.AmountPaid)
				.HasPrecision(18, 2)
				.HasDefaultValue(0m)
				.IsRequired();

			b.Property(x => x.Currency)
				.HasColumnType("char(3)")
				.HasDefaultValue("VND")
				.IsRequired();

			b.Property(x => x.CheckoutUrl)
				.HasColumnType("text");

			b.Property(x => x.QrCode)
				.HasColumnType("text");

			b.Property(x => x.ReturnUrl)
				.HasColumnType("text");

			b.Property(x => x.CancelUrl)
				.HasColumnType("text");

			b.Property(x => x.CancellationReason)
				.HasColumnType("text");

			b.Property(x => x.TenantId)
				.IsRequired();

			b.Property(x => x.RequestBy)
				.IsRequired();

			b.Property(x => x.ProviderConfigId)
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.PaymentOrders)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.User)
				.WithMany(x => x.PaymentOrders)
				.HasForeignKey(x => new { x.TenantId, x.RequestBy })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.PaymentProviderConfig)
				.WithMany(x => x.PaymentOrders)
				.HasForeignKey(x => new { x.TenantId, x.ProviderConfigId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.TenantInvoice)
				.WithMany()
				.HasForeignKey(x => new { x.TenantId, x.TenantInvoiceId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasIndex(x => x.OrderCode)
				.IsUnique();

			b.HasIndex(x => x.ProviderPaymentLinkId)
				.IsUnique();

			b.HasIndex(x => new {x.TenantId, x.Status, x.ExpiresAt})
				.HasDatabaseName("payment_orders_idx")
				.HasFilter("\"status\" IN ('pending'::tenant.payment_order_status, 'underpaid'::tenant.payment_order_status)");

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"CK_payment_orders_amount_positive",
					"amount > 0");

				x.HasCheckConstraint(
					"CK_payment_orders_amount_paid_nonnegative",
					"amount_paid >= 0");

				x.HasCheckConstraint(
					"CK_payment_orders_amount_paid_not_exceed_amount",
					"amount_paid <= amount");
			});

			b.HasAlternateKey(x => new { x.TenantId, x.Id });
		}
	}
}
