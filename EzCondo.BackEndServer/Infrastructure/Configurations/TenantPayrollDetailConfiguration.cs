using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class TenantPayrollDetailConfiguration()
		: EntityConfigurationBase<TenantPayrollDetail>("tenant_payroll_details", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<TenantPayrollDetail> b)
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

			b.Property(x => x.NetPay)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.Property(x => x.TenantId)
				.IsRequired();

			b.Property(x => x.RecipientId)
				.IsRequired();

			b.Property(x => x.PayrollId)
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.TenantPayrollDetails)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.User)
				.WithMany(x => x.TenantPayrollDetails)

				.HasForeignKey(x => new { x.TenantId, x.RecipientId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.TenantPayroll)
				.WithMany(x => x.TenantPayrollDetails)
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

				x.HasCheckConstraint(
					"CK_tenant_payroll_details_net_pay",
					"net_pay >= 0"
					);
			});

			b.HasIndex(x => new { x.TenantId, x.Id }).IsUnique();

			b.HasIndex(x => new { x.TenantId, x.PayrollId, x.RecipientId }).IsUnique();
		}
	}
}
