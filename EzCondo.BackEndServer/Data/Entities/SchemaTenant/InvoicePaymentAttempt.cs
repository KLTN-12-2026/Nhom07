using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaPayment;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaTenant
{
	public sealed class InvoicePaymentAttempt //Cư dân thanh toán hóa đơn
	{
		public Guid Id { get; set; }
		public Guid TenantId { get; set; }
		public Guid InvoiceId { get; set; }
		public Guid GatewayAccountId { get; set; }
		public Guid GatewayPaymentRequestId { get; set; }
		public Guid RequestedByUserId { get; set; }
		public InvoicePaymentAttemptStatus Status { get; set; } = InvoicePaymentAttemptStatus.Pending;
		public DateTimeOffset CreatedAt { get; set; }

		public Tenant Tenant { get; set; } = null!;
		public Invoice Invoice { get; set; } = null!;
		public ProviderAccount ProviderAccount { get; set; } = null!;
		public PaymentRequest PaymentRequest { get; set; } = null!;
		public User User { get; set; } = null!;

		public ICollection<InvoicePaymentAllocation> InvoicePaymentAllocations { get; set; } = new List<InvoicePaymentAllocation>();

	}
}
