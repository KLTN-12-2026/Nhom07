using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaTenant
{
	internal sealed class InvoiceDetailConfiguration()
		: EntityConfigurationBase<InvoiceDetail>("invoice_details", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<InvoiceDetail> b)
		{
			ConfigureUuidPrimaryKey(b);

			b.Property(x => x.InvoiceId)
				.IsRequired();

			b.Property(x => x.Quantity)
				.IsRequired();

			b.Property(x => x.UnitPrice)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.Property(x => x.Amount)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.Property(x => x.Description)
				.HasColumnType("text");

			b.HasOne(x => x.Invoice)
				.WithMany(x => x.InvoiceDetails)
				.HasForeignKey(x => new {x.TenantId, x.InvoiceId})
				.HasPrincipalKey(x=> new {x.TenantId, x.Id})
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.TenantService)
				.WithMany(x => x.TenantInvoiceLines)
				.HasForeignKey(x => new { x.TenantId, x.ServiceId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.InvoiceDetails)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
