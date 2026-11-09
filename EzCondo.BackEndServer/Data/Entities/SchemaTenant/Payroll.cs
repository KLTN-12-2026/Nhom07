using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaTenant
{
	public class Payroll : ISoftDeletableEntity, IAuditableEntity
	{
		public Guid Id { get; set; }
		public string Title { get; set; } = null!;
		public PayrollStatusEnum Status { get; set; } = PayrollStatusEnum.Draft;
		public int Version { get; set; } = 1;
		public decimal Netpay { get; set; } //thực nhận
		public Guid CreatedBy { get; set; }
		public Guid TenantId { get; set; }
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public DateTimeOffset? PublishedAt { get; set; }

		public Tenant Tenant { get; set; } = null!;
		public User ManagerCreated { get; set; } = null!;

		public ICollection<PayrollDisbursement> PayrollDisbursements { get; set; } = new List<PayrollDisbursement>();

		public ICollection<PayrollDetail> PayrollDetails { get; set; } = new List<PayrollDetail>();
	}
}
