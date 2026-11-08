using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaTenant
{
	internal sealed class InvoiceConfiguration()
		: EntityConfigurationBase<Invoice>("invoices", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<Invoice> b)
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

			b.Property(x => x.PublishedAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.BillingPeriod)
				.HasColumnType("timestamptz")
				.IsRequired();

			b.Property(x => x.TotalAmount)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.Property(x => x.PaymentDealineAt)
				.HasColumnType("timestamptz")
				.IsRequired();

			b.Property(x => x.Note)
				.HasColumnType("text");

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.Invoices)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.Apartment)
				.WithMany(x => x.Invoices)
				.HasForeignKey(x => new { x.TenantId, x.ApartmentId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.StaffCreated)
				.WithMany(x => x.CreatedByStaff)
				.HasForeignKey(x => new { x.TenantId, x.CreatedByStaff })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.StaffUpdated)
				.WithMany(x => x.UpdatedByStaff)
				.HasForeignKey(x => new { x.TenantId, x.UpdatedByStaff })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.StaffDeleted)
				.WithMany(x => x.DeletedByStaff)
				.HasForeignKey(x => new { x.TenantId, x.DeletedByStaff})
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.ResidentPaid)
				.WithMany(x => x.PaidByResident)
				.HasForeignKey(x => new { x.TenantId, x.PaidByResident})
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasIndex(x => new { x.TenantId, x.InvoiceNo, x.Id })
				.IsUnique();

			b.ToTable(x =>
			{
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

			b.HasIndex(x => new { x.TenantId, x.InvoiceNo })
				.IsUnique();

			b.HasAlternateKey(x => new { x.TenantId, x.Id });
		}
	}
}
