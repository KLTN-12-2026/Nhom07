using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;

namespace EzCondo.BackEndServer.Data.Entities.SchemaTenant
{
	public class PayrollDetail
	{
		public Guid Id { get; set; }
		public Decimal BaseSalary { get; set; }
		public Decimal Allowance { get; set; } //Các khoản phụ cấp
		public Decimal Deduction { get; set; } //Khấu trừ bảo hiểm
		public Guid StaffReceive { get; set; }
		public Guid PayrollId { get; set; }
		public Guid TenantId { get; set; }

		public Tenant Tenant { get; set; }
		public User Staff { get; set; }
		public Payroll Payroll { get; set; }
	}
}
