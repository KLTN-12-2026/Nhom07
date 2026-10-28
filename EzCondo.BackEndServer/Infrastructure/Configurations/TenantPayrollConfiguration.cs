using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class TenantPayrollConfiguration()
		: EntityConfigurationBase<TenantPayroll>("tenant_payrolls", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<TenantPayroll> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);
			ConfigureDeletedAt(b);
			ConfigureIsDeleted(b);

			b.Property(x => x.Title)
				.HasMaxLength(200)
				.IsRequired();

			b.Property(x => x.TotalAmount)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.Property(x => x.Status)
				.HasColumnType("tenant.payroll_status_enum")
				.HasDefaultValueSql("'draft'::tenant.payroll_status_enum")
				.IsRequired();

			b.Property(x => x.TenantId)
				.IsRequired();

			b.Property(x => x.CreatedBy)
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.TenantPayrolls)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.Manager)
				.WithMany(x => x.TenantPayrolls)
				.HasForeignKey(x => new { x.TenantId, x.CreatedBy })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.ManagerSubmitted)
				.WithMany()
				.HasForeignKey(x => new { x.TenantId, x.SubmittedBy })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.ApprovedByUser)
				.WithMany()
				.HasForeignKey(x => new { x.TenantId, x.ApprovedBy })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.RejectedByUser)
				.WithMany()
				.HasForeignKey(x => new { x.TenantId, x.RejectedBy })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.IssuedByUser)
				.WithMany()
				.HasForeignKey(x => new { x.TenantId, x.IssuedBy })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.ToTable(x => x.HasCheckConstraint(
				"CK_tenant_payrolls_total_amount_positive",
				"total_amount >= 0"
				));

			b.HasIndex(x => new { x.TenantId, x.Id }).IsUnique();

			b.HasAlternateKey(x => new { x.TenantId, x.Id });
		}
	}
}
