using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaTenant
{
	internal sealed class StaffTimeSheetConfiguration()
		: EntityConfigurationBase<StaffTimeSheet>("staff_time_sheets", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<StaffTimeSheet> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);
			ConfigureDeletedAt(b);
			ConfigureIsDeleted(b);

			b.Property(x => x.StandardWorkingDay)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.Property(x => x.PaidLeaveDay)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.Property(x => x.UnpaidLeaveDay)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.Property(x => x.OvertimeHourNormal)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.Property(x => x.OvertimeHourWeekend)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.Property(x => x.OvertimeHourHoliday)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.Property(x => x.BonusAmount)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.Property(x => x.PenaltyAmount)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.Property(x => x.Note)
				.HasColumnType("text");

			b.HasOne(x => x.Tenant)
				.WithMany()
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.Staff)
				.WithMany(x => x.Staff)
				.HasForeignKey(x => new { x.TenantId, x.StaffId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.Manager)
				.WithMany(x => x.ManagerCreated)
				.HasForeignKey(x => new { x.TenantId, x.CreatedByManager })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"CK_tenant_staff_time_sheets_standard_working_day_positive",
					"standard_working_day >= 0 AND standard_working_day <= 30"
					);

				x.HasCheckConstraint(
					"CK_tenant_staff_time_sheets_paid_leave_day_positive",
					"paid_leave_day >= 0"
					);

				x.HasCheckConstraint(
					"CK_tenant_staff_time_sheets_unpaid_leave_day_positive",
					"unpaid_leave_day >= 0"
					);

				x.HasCheckConstraint(
					"CK_tenant_staff_time_sheets_overtime_hour_normal_positive",
					"overtime_hour_normal >= 0"
					);

				x.HasCheckConstraint(
					"CK_tenant_staff_time_sheets_overtime_hour_weekend_positive",
					"overtime_hour_weekend >= 0"
					);

				x.HasCheckConstraint(
					"CK_tenant_staff_time_sheets_overtime_hour_holiday_positive",
					"overtime_hour_holiday >= 0"
					);

				x.HasCheckConstraint(
					"CK_tenant_staff_time_sheets_bonus_amount_positive",
					"bonus_amount >= 0"
					);

				x.HasCheckConstraint(
					"CK_tenant_staff_time_sheets_penalty_amount_positive",
					"penalty_amount >= 0"
					);
			});

			b.HasIndex(x => new { x.TenantId, x.PayrollPeriod, x.StaffId }).IsUnique();
		}
	}
}
