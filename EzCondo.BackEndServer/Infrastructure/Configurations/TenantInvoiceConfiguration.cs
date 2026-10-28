using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class TenantInvoiceConfiguration()
		: EntityConfigurationBase<TenantInvoice>("tenant_invoices", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<TenantInvoice> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);
			ConfigureIsDeleted(b);
			ConfigureDeletedAt(b);

			b.Property(x => x.InvoiceNo)
				.HasMaxLength(80)
				.IsRequired();

			b.Property(x => x.Status)
				.HasColumnType("tenant.tenant_invoice_status")
				.HasDefaultValueSql("'draft'::tenant.tenant_invoice_status")
				.IsRequired();

			b.Property(x => x.Currency)
				.HasColumnType("char(3)")
				.HasDefaultValue("VND")
				.IsRequired();

			b.Property(x => x.SubtotalAmount)
				.HasColumnType("numeric")
				.HasPrecision(18, 2)
				.HasDefaultValue(0m);

			b.Property(x => x.AdjustmentAmount)
				.HasColumnType("numeric")
				.HasPrecision(18, 2)
				.HasDefaultValue(0m)
				.IsRequired();

			b.Property(x => x.TotalAmount)
				.HasColumnType("numeric")
				.HasPrecision(18, 2)
				//.HasDefaultValue(0m)
				.HasComputedColumnSql("subtotal_amount + adjustment_amount", stored: true)
				.IsRequired();

			b.Property(x => x.PaidAmount)
				.HasColumnType("numeric")
				.HasPrecision(18, 2)
				.HasDefaultValue(0m)
				.IsRequired();

			b.Property(x => x.Note)
				.HasColumnType("text");

			b.Property(x => x.TenantId)
				.IsRequired();

			b.Property(x => x.BillingPeriodId)
				.IsRequired();

			b.Property(x => x.ApartmentId)
				.IsRequired();

			b.Property(x => x.BillingContactId)
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.TenantInvoices)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.TenantBillingPeriod)
				.WithMany(x => x.TenantInvoices)
				.HasForeignKey(x => new {x.TenantId, x.BillingPeriodId })
				.HasPrincipalKey(x => new {x.TenantId, x.Id})
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.TenantApartment)
				.WithMany(x => x.TenantInvoices)
				.HasForeignKey(x => new { x.TenantId, x.ApartmentId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.TenantResident)
				.WithMany()
				.HasForeignKey(x => new { x.TenantId, x.BillingContactId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasIndex(x => new { x.TenantId, x.InvoiceNo })
				.IsUnique();

			b.HasIndex(x => new { x.TenantId, x.Id })
				.IsUnique();

			b.HasIndex(x => new { x.BillingPeriodId, x.ApartmentId })
				.IsUnique()
				.AreNullsDistinct(false);

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"CK_tenant_invoices_paid_amount_positive",
					"paid_amount >= 0"
					);

				x.HasCheckConstraint(
					"CK_tenant_invoices_subtotal_amount_positive",
					"subtotal_amount >= 0"
					);

				x.HasCheckConstraint(
					"CK_tenant_invoices_paid_le_total",
					"paid_amount <= total_amount"
					);

				//Cấu hình trong BE: payroll.total_amount = SUM(detail.net_pay)
			});

			b.HasAlternateKey(x => new { x.TenantId, x.Id });
		}
	}
}
