using EzCondo.BackEndServer.Data.Entities.SchemaPayment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaPayment
{
	internal sealed class PaymentRequestConfiguration()
		: EntityConfigurationBase<PaymentRequest>("payment_requests", "payment")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<PaymentRequest> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);

			b.Property(x => x.OrderCode)
				.HasColumnType("bigint")
				.IsRequired();

			b.Property(x => x.ProviderPaymentLinkId)
				.HasMaxLength(255);

			b.Property(x => x.Status)
				.HasColumnType("payment.payment_request_status")
				.HasDefaultValueSql("'pending'::payment.payment_request_status")
				.IsRequired();

			b.Property(x => x.Amount)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.Property(x => x.AmountRecived)
				.HasColumnType("numeric(18,2)")
				.HasDefaultValue(0)
				.IsRequired();

			b.Property(x => x.Currency)
				.HasColumnType("char(3)")
				.HasDefaultValue("'VND'")
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

			b.Property(x => x.ExpiredAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.CancelledAt)
				.HasColumnType("timestamptz");

			b.HasOne(x => x.ProviderAccounts)
				.WithMany(x => x.PaymentRequests)
				.HasForeignKey(x => x.ProviderAccountId)
				.OnDelete(DeleteBehavior.Restrict);

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"ck_payment_requests_order_code",
					"CHECK (order_code > 0)");

				x.HasCheckConstraint(
					"ck_payment_requests_amount",
					"CHECK (amount > 0)");

				x.HasCheckConstraint(
					"ck_payment_requests_amount_received",
					"CHECK (amount_received > 0)");

				x.HasCheckConstraint(
					"ck_payment_requests_integer_vnd",
					"CHECK (amount = trunc(amount) AND amount_received = trunc(amount_received))");
			});

			b.HasIndex(x => new { x.ProviderAccountId, x.OrderCode })
				.IsUnique();

			b.HasIndex(x => new { x.ProviderAccountId, x.Id })
				.IsUnique();

			b.HasIndex(x => new { x.ProviderAccountId, x.ProviderPaymentLinkId })
				.HasDatabaseName("ux_payment_requests__provider_link")
				.IsUnique()
				.HasFilter("provider_payment_link_id IS NOT NULL");

			b.HasIndex(x => new { x.ProviderAccountId, x.Status, x.ExpiredAt })
				.HasDatabaseName("ix_payment_requests_request_open")
				.HasFilter("status IN ('peding', 'underpaid')");

			b.HasIndex(x => new { x.ProviderAccountId, x.CreatedAt })
				.HasDatabaseName("ix_payment_requests_created")
				.IsDescending(false, true);
		}
	}
}
