using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class SubscriptionInvoiceConfiguration()
		: EntityConfigurationBase<SubscriptionInvoice>("subscription_invoices", "platform")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<SubscriptionInvoice> b)
		{
			ConfigureUuidPrimaryKey(b);

			b.Property(x => x.InvoiceNo)
				.HasMaxLength(50)
				.IsRequired();

			b.Property(x => x.Period)
				.HasColumnType("daterange")
				.IsRequired();

			b.Property(x => x.Amount)
				.HasPrecision(18, 2)
				.IsRequired();

			b.Property(x => x.DueAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.PaidAmount)
				.HasPrecision(18, 2)
				.HasDefaultValue(0m)
				.IsRequired();

			b.Property(x => x.Status)
				.HasColumnType("platform.subscription_invoice_status")
				.HasDefaultValueSql("'draft'::platform.subscription_invoice_status")
				.IsRequired();

			ConfigureCreatedAt(b);

			b.Property(x => x.IssuedAt)
				.HasColumnType("timestamptz");

			b.HasIndex(x => x.InvoiceNo)
				.IsUnique();

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"CK_subscription_invoices_amount_positive",
					"amount >= 0"
					);

				x.HasCheckConstraint(
					"CK_subscription_invoices_paid_amount_positive",
					"paid_amount >= 0"
					);

				x.HasCheckConstraint(
					"CK_subscription_invoices_period_not_empty",
					"NOT isempty(period)"
					);

				x.HasCheckConstraint(
					"CK_subscription_invoice_paid_le_amount",
					"paid_amount <= amount"
					);
			});

			b.HasOne(x => x.Subscription)
				.WithMany(x => x.SubscriptionInvoices)
				.HasForeignKey(x => x.SubscriptionId)
				.OnDelete(DeleteBehavior.NoAction);
		}
	}
}
