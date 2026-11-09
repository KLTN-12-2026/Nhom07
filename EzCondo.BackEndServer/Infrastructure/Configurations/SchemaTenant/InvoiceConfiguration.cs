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
				.IsRequired();

			b.Property(x => x.PublishedAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.BillingPeriod)
				.HasColumnType("timestamptz")
				.IsRequired();

			b.Property(x => x.TotalAmount)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.Property(x => x.PaymentDeadlineAt)
				.HasColumnType("timestamptz")
				.IsRequired();

			b.Property(x => x.Note)
				.HasColumnType("text");

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.Invoices)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.Apartment)
				.WithMany(x => x.Invoices)
				.HasForeignKey(x => new { x.TenantId, x.ApartmentId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.StaffCreated)
				.WithMany(x => x.CreatedByStaff)
				.HasForeignKey(x => x.CreatedByStaff)
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.StaffUpdated)
				.WithMany(x => x.UpdatedByStaff)
				.HasForeignKey(x => x.UpdatedByStaff)
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.StaffDeleted)
				.WithMany(x => x.DeletedByStaff)
				.HasForeignKey(x => x.DeletedByStaff)
				.OnDelete(DeleteBehavior.NoAction);

			b.HasIndex(x => new { x.TenantId, x.InvoiceNo })
				.IsUnique();

			b.HasAlternateKey(x => new { x.TenantId, x.Id });
		}
	}
}
