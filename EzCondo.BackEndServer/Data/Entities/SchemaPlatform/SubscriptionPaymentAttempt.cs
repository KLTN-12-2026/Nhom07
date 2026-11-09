using EzCondo.BackEndServer.Data.Entities.SchemaPayment;
using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaPlatform
{
	public sealed class SubscriptionPaymentAttempt //1 link payment = 1 attempt
	{
		public Guid Id { get; set; }
		public Guid SubscriptionInvoiceId { get; set; }
		public Guid GatewayAccountId { get; set; }
		public Guid GatewayPaymentRequestId { get; set; }
		public Guid RequestedByUserId { get; set; }
		public SubscriptionPaymentAttemptStatus Status { get; set; } = SubscriptionPaymentAttemptStatus.Pending;
		public DateTimeOffset CreatedAt { get; set; }

		public SubscriptionInvoice SubscriptionInvoice { get; set; } = null!;
		public ProviderAccount ProviderAccount { get; set; } = null!;
		public PaymentRequest PaymentRequests { get; set; } = null!;
		public User RequestedByUser { get; set; } = null!;

		public ICollection<SubscriptionPaymentAllocation> SubscriptionPaymentAllocations { get; set; } = new List<SubscriptionPaymentAllocation>();

	}
}
