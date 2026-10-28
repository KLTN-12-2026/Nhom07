using EzCondo.BackEndServer.Data.Enums;
using System.Text.Json;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class TenantPaymentTransaction
	{
		public Guid Id { get; set; }
		public string ProviderReference { get; set; }
		public string ProviderTransactionId { get; set; }
		public PaymentTransactionStatus Status { get; set; }
		public Decimal Amount { get; set; }
		public DateTimeOffset OccurredAt { get; set; }
		public string PayerMasked { get; set; }
		public string ProviderPayload { get; set; }
		public DateTimeOffset CreatedAt { get; set; }

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }
		public Guid PaymentOrderId { get; set; }
		public TenantPaymentOrder PaymentOrdrer { get; set; }
	}
}
