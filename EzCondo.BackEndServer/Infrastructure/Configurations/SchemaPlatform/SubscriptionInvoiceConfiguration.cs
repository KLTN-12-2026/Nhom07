using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaPlatform
{
	internal sealed class SubscriptionInvoiceConfiguration()
		: EntityConfigurationBase<SubscriptionInvoice>("subscription_invoices", "platform")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<SubscriptionInvoice> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);

			b.Property(x => x.InvoiceNo)
				.HasMaxLength(80)
				.IsRequired();

			b.Property(x => x.BillingPeriodFrom)
				.HasColumnType("date");
			
			b.Property(x => x.BillingPeriodTo)
				.HasColumnType("date");

			b.Property(x => x.DueAt)
				.HasColumnType("timestamptz")
				.IsRequired();

			b.Property(x => x.Status)
				.HasColumnType("platform.subscription_invoice_status")
				.IsRequired();

			b.Property(x => x.TotalAmount)
				.HasColumnType("numeric(18,2)")
				.IsRequired();
			
			b.Property(x => x.PaidAmount)
				.HasColumnType("numeric(18,2)")
				.HasDefaultValue(0)
				.IsRequired();

			b.Property(x => x.Currency)
				.HasColumnType("char(3)")
				.HasDefaultValue("VND")
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.SubscriptionInvoices)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.Subscription)
				.WithMany(x => x.SubscriptionInvoices)
				.HasForeignKey(x => new {x.TenantId, x.SubscriptionId })
				.HasPrincipalKey(x => new {x.TenantId, x.Id})
				.OnDelete(DeleteBehavior.Restrict);

			b.HasIndex(x => new { x.TenantId, x.InvoiceNo })
				.IsUnique();

			b.HasIndex(x => new { x.TenantId, x.Status, x.DueAt })
				.HasDatabaseName("ix_subscription_invoices_due")
				.HasFilter("status IN ('issued', 'partially_paid', 'overdue')");

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"ck_subscription_invoices_amount",
					"(total_amount > 0 AND paid_amount >= 0 AND paid_amount <= total_amount)");
				x.HasCheckConstraint(
					"ck_subscription_invoices_billing_period",
					"(billing_period_from IS NULL AND billing_period_to IS NULL) OR " +
					"(billing_period_from IS NOT NULL AND billing_period_to IS NOT NULL AND billing_period_to > billing_period_from)");
			});
		}
	}
}
