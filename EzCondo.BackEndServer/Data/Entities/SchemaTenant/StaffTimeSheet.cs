using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;

namespace EzCondo.BackEndServer.Data.Entities.SchemaTenant
{
	public class StaffTimeSheet : ISoftDeletableEntity, IAuditableEntity //Tổng hợp công nhân viên
	{
		public Guid Id { get; set; }
		public DateTimeOffset PayrollPeriod { get; set; }
		public Decimal StandardWorkingDay { get; set; } //Số ngày công tiêu chuẩn
		public Decimal PaidLeaveDay { get; set; } //Nghỉ phép hưởng lương
		public Decimal UnpaidLeaveDay { get; set; } //nghỉ phép ko lương
		public Decimal OvertimeHourNormal { get; set; } //Giờ OT ngày thường
		public Decimal OvertimeHourWeekend { get; set; } //Giờ OT cuối tuần
		public Decimal OvertimeHourHoliday { get; set; } //Giờ OT ngày lễ
		public Decimal BonusAmount { get; set; } //Thưởng phát sinh
		public Decimal PenaltyAmount { get; set; } //Phạt phát sinh
		public string? Note { get; set; }

		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public Guid CreatedByManager { get; set; }
		public Guid StaffId { get; set; }
		public Guid TenantId { get; set; }

		public Tenant Tenant { get; set; } = null!;
		public User Manager { get; set; } = null!;
		public User Staff { get; set; } = null!;
	}
}
