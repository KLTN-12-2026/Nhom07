using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaPayment;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaTenant
{
	public sealed class PayrollDisbursementItem : IAuditableEntity
	{
		public Guid Id { get; set; }
		public Guid TenantId { get; set; }
		public Guid PayrollDisbursementId { get; set; }
		public Guid PayrollDetailId { get; set; }
		public Guid StaffUserId { get; set; }
		public Guid GatewayAccountId { get; set; }
		public Guid? GatewayPayoutItemId { get; set; }
		public decimal Amount { get; set; }
		public string DestinationBin { get; set; } = null!;
		public byte[] DestinationAccountCiphertext { get; set; } = null!;
		public string DestinationAccountLast4 { get; set; } = null!;
		public string RecipientNameSnapshot { get; set; } = null!;
		public PayrollDisbursementItemStatus Status { get; set; } = PayrollDisbursementItemStatus.Ready;
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public Tenant Tenant { get; set; } = null!;
		public PayrollDisbursement PayrollDisbursement { get; set; } = null!;
		public PayrollDetail PayrollDetail { get; set; } = null!;
		public User StaffUser { get; set; } = null!;
		public ProviderAccount ProviderAccount { get; set; } = null!;
		public PayoutItem? PayoutItem { get; set; }
	}
}
