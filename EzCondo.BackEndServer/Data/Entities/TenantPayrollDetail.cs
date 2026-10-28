namespace EzCondo.BackEndServer.Data.Entities
{
	public class TenantPayrollDetail
	{
		public Guid Id { get; set; }
		public Decimal BaseSalary { get; set; }
		public Decimal Allowance { get; set; } //Các khoản phụ cấp
		public Decimal Deduction { get; set; } //Khấu trừ bảo hiểm
		public Decimal NetPay { get; set; } //Lương thực lãnh

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }
		public Guid RecipientId { get; set; } //Người nhận
		public TenantStaffProfile User { get; set; }
		public Guid PayrollId { get; set; }
		public TenantPayroll TenantPayroll { get; set; }
	}
}
