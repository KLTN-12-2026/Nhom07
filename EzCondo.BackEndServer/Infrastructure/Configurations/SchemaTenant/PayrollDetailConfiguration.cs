using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaTenant
{
	internal sealed class PayrollDetailConfiguration()
		: EntityConfigurationBase<PayrollDetail>("payroll_details", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<PayrollDetail> b)
		{
			ConfigureUuidPrimaryKey(b);

			b.Property(x => x.BaseSalary)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.Property(x => x.Allowance)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.Property(x => x.Deduction)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.PayrollDetails)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.Staff)
				.WithMany(x => x.PayrollDetails)
				.HasForeignKey(x => x.StaffReceive)
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.Payroll)
				.WithMany(x => x.PayrollDetails)
				.HasForeignKey(x => new { x.TenantId, x.PayrollId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"CK_tenant_payroll_details_base_salary_positive",
					"base_salary > 0"
					);

				x.HasCheckConstraint(
					"CK_tenant_payroll_details_allowance_positive",
					"allowance >= 0"
					);

				x.HasCheckConstraint(
					"CK_tenant_payroll_details_deduction_positive",
					"deduction >= 0"
					);
			});

			b.HasAlternateKey(x => new { x.TenantId, x.Id });

			b.HasIndex(x => new { x.TenantId, x.PayrollId, x.StaffReceive }).IsUnique();
		}
	}
}
