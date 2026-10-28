using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class TenantInvoiceLineConfiguration()
		: EntityConfigurationBase<TenantInvoiceLine>("tenant_invoice_lines", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<TenantInvoiceLine> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);

			b.Property(x => x.Description)
				.HasMaxLength(500)
				.IsRequired();

			b.Property(x => x.Quantity)
				.HasColumnType("numeric(14,3)")
				.HasDefaultValue(1m)
				.IsRequired();

			b.Property(x => x.Unit)
				.HasMaxLength(30)
				.IsRequired();

			b.Property(x => x.UnitPrice)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.Property(x => x.LineAmount)
				.HasColumnType("numeric(18,2)")
				.HasComputedColumnSql("round(quantity * unit_price, 2)", stored: true)
				.IsRequired();

			b.Property(x => x.InvoiceId)
				.IsRequired();

			b.Property(x => x.ServiceId)
				.IsRequired();
			b.HasOne(x => x.TenantInvoice)
				.WithMany(x => x.TenantInvoiceLines)
				.HasForeignKey(x => new { x.TenantId, x.InvoiceId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.TenantService)
				.WithMany(x => x.TenantInvoiceLines)
				.HasForeignKey(x => new { x.TenantId, x.ServiceId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.Tenant)
				.WithMany()
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"CK_tenant_invoice_lines_quantity_positive",
					"quantity >= 0"
					);

				x.HasCheckConstraint(
					"CK_tenant_invoice_lines_unit_price_positive",
					"unit_price >= 0"
					);
			});
		}
	}
}
