using EzCondo.BackEndServer.Data.Entities;
using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaPayment
{
	public sealed class PayoutBatch : IAuditableEntity
	{
		public Guid Id { get; set; }
		public Guid ProviderAccountId { get; set; }
		public string ReferenceId { get; set; } = null!;
		public string IdempotencyKey { get; set; } = null!;
		public string? ProviderPayoutId { get; set; }
		public PayoutBatchStatus Status { get; set; } = PayoutBatchStatus.Created;
		public string? ProviderStatus { get; set; }
		public decimal TotalAmount { get; set; }
		public decimal? EstimatedFee { get; set; }
		public string Currency { get; set; } = null!;
		public DateTimeOffset? LastSyncedAt { get; set; }
		public string ProviderPayload { get; set; } = null!;
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public ProviderAccount ProviderAccount { get; set; } = null!;

		public ICollection<PayoutItem> PayoutItems { get; set; } = new List<PayoutItem>();
		public ICollection<PayrollDisbursement> PayrollDisbursements { get; set; } = new List<PayrollDisbursement>();

	}
}
