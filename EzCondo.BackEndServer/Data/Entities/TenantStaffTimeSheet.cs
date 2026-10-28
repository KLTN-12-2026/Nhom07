using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class TenantStaffTimeSheet : ISoftDeletableEntity, IAuditableEntity //Tổng hợp công nhân viên
	{
		public Guid Id { get; set; }
		public int DateMonth { get; set; }
		public int DateYear { get; set; }
		public Decimal StandardWorkingDay { get; set; } //Số ngày công tiêu chuẩn
		public Decimal ActualWorkingDay { get; set; } //Số ngày công thực tế
		public Decimal PaidLeaveDay { get; set; } //Nghỉ phép hưởng lương
		public Decimal UnpaidLeaveDay { get; set; } //nghỉ phép ko lương
		public Decimal OvertimeHourNormal { get; set; } //Giờ OT ngày thường
		public Decimal OvertimeHourWeekend { get; set; } //Giờ OT cuối tuần
		public Decimal OvertimeHourHoliday { get; set; } //Giờ OT ngày lễ
		public Decimal NightShiftsCount { get; set; } //Số ca trực đêm
		public Decimal BonusAmount { get; set; } //Thưởng phát sinh
		public Decimal PenaltyAmount { get; set; } //Phạt phát sinh
		public string? Note { get; set; }
		public TimeSheetStatus Status { get; set; }
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }
		public Guid StaffId { get; set; }
		public TenantStaffProfile TenantStaffProfile { get; set; }
	}
}
