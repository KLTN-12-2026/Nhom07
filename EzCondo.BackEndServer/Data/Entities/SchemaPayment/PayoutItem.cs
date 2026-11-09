using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaPayment
{
	public sealed class PayoutItem : IAuditableEntity
	{
		public Guid Id { get; set; }
		public Guid ProviderAccountId { get; set; }
		public Guid PayoutBatchId { get; set; }
		public string ReferenceId { get; set; } = null!;
		public string? ProviderTransactionId { get; set; }
		public decimal Amount { get; set; }
		public string Currency { get; set; } = null!;
		public string Description { get; set; } = null!;
		public string DestinationBin { get; set; } = null!;
		public byte[] DestinationAccountCiphertext { get; set; } = null!;
		public string DestinationAccountLast4 { get; set; } = null!;
		public string DestinationAccountName { get; set; } = null!;
		public PayoutItemStatusEnum Status { get; set; } = PayoutItemStatusEnum.Created;
		public string? ProviderStatus { get; set; }
		public string? FailureCode { get; set; }
		public string? FailureMessage { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public ProviderAccount ProviderAccount { get; set; } = null!;
		public PayoutBatch PayoutBatch { get; set; } = null!;

		public ICollection<PayrollDisbursementItem> PayrollDisbursementItems { get; set; } = new List<PayrollDisbursementItem>();

	}
}
