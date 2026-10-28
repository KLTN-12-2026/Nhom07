using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class TenantPaymentOrder : IAuditableEntity //Lệnh thanh toán
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
		public IdentityUser User { get; set; }
		public Guid ProviderConfigId { get; set; }
		public TenantPaymentProviderConfig PaymentProviderConfig { get; set; }
		public Guid TenantInvoiceId { get; set; }
		public TenantInvoice TenantInvoice { get; set; } = null!;

		public ICollection<TenantPaymentTransaction> PaymentTransactions { get; set; } = new List<TenantPaymentTransaction>();
	}
}
