using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaTenant
{
	internal sealed class PayrollConfiguration()
		: EntityConfigurationBase<Payroll>("payrolls", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<Payroll> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);
			ConfigureDeletedAt(b);
			ConfigureIsDeleted(b);

			b.Property(x => x.Title)
				.HasMaxLength(200)
				.IsRequired();

			b.Property(x => x.Netpay)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.Property(x => x.Status)
				.HasColumnType("tenant.payroll_status_enum")
				.IsRequired();

			b.Property(x => x.Version)
				.HasDefaultValue(1)
				.IsRequired();

			b.Property(x => x.PublishedAt)
				.HasColumnType("timestamptz");

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.Payrolls)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.ManagerCreated)
				.WithMany(x => x.Payrolls)
				.HasForeignKey(x => x.CreatedBy)
				.OnDelete(DeleteBehavior.NoAction);

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"CK_tenant_payrolls_netpay_positive",
					"netpay >= 0");
				x.HasCheckConstraint(
					"ck_payrolls_version",
					"version > 0");
			});

			b.HasAlternateKey(x => new { x.TenantId, x.Id });
		}
	}
}
