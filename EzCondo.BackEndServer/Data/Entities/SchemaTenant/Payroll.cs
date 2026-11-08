using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;

namespace EzCondo.BackEndServer.Data.Entities.SchemaTenant
{
	public class Payroll : ISoftDeletableEntity, IAuditableEntity
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
		public PayrollStatusEnum Status { get; set; }
		public decimal Netpay { get; set; } //thực nhận
		public Guid CreatedBy { get; set; }
		public Guid TenantId { get; set; }
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public DateTimeOffset? PublishedAt { get; set; }

		public Tenant Tenant { get; set; }
		public User ManagerCreated { get; set; }

		public ICollection<PayrollDetail> PayrollDetails { get; set; } = new List<PayrollDetail>();
	}
}
