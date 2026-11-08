using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaPayment
{
	public class PaymentOrder : IAuditableEntity //Lệnh thanh toán
	{
		public Guid Id { get; set; }
		public long OrderCode { get; set; }
		public string ProviderPaymentLinkId { get; set; }
		public PaymentOrderStatus Status { get; set; }
		public Decimal Amount { get; set; }
		public Decimal AmountPaid { get; set; }
		public string Currency { get; set; }
		public string? CheckoutUrl { get; set; }
		public string? QrCode { get; set; }
		public string? ReturnUrl { get; set; }
		public string? CancelUrl { get; set; }
		public DateTimeOffset? ExpiresAt { get; set; }
		public DateTimeOffset? CancelledAt { get; set; }
		public string? CancellationReason { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }
		public Guid RequestBy { get; set; }
		public User User { get; set; }
		public Guid ProviderConfigId { get; set; }
		public PaymentProviderConfig PaymentProviderConfig { get; set; }
		public Guid TenantInvoiceId { get; set; }
		public Invoice TenantInvoice { get; set; } = null!;

		public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
	}
}
