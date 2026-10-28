
namespace EzCondo.BackEndServer.Data.Entities
{
	public class TenantPaymentProviderConfig : IAuditableEntity
	{
		public Guid Id { get; set; }
		public string? MerchantCode { get; set; }
		public string SecretReference { get; set; }
		public string Settings { get; set; }
		public bool IsActive { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }

		public ICollection<TenantPaymentOrder> PaymentOrders { get; set; } = new List<TenantPaymentOrder>();
	}
}
