using EzCondo.BackEndServer.Data.Entities;
using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaPayment;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaTenant
{
	public sealed class PayrollDisbursement : IAuditableEntity
	{
		public Guid Id { get; set; }
		public Guid TenantId { get; set; }
		public Guid PayrollId { get; set; }
		public Guid GatewayAccountId { get; set; }
		public Guid? GatewayPayoutBatchId { get; set; }
		public int ApprovedPayrollVersion { get; set; }
		public byte[] ApprovedContentHash { get; set; } = null!;
		public Guid ApprovedByUserId { get; set; }
		public DateTimeOffset ApprovedAt { get; set; }
		public Guid? IssuedByUserId { get; set; }
		public DateTimeOffset? IssuedAt { get; set; }
		public PayrollDisbursementStatus Status { get; set; } = PayrollDisbursementStatus.Ready;
		public decimal TotalAmount { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public Tenant Tenant { get; set; } = null!;
		public Payroll Payroll { get; set; } = null!;
		public ProviderAccount ProviderAccount { get; set; } = null!;
		public PayoutBatch? PayoutBatch { get; set; }
		public User ApprovedByUser { get; set; } = null!;
		public User? IssuedByUser { get; set; }

		public ICollection<PayrollDisbursementItem> PayrollDisbursementItems { get; set; } = new List<PayrollDisbursementItem>();

	}
}
