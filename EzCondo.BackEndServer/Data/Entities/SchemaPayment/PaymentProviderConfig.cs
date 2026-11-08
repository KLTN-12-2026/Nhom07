using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;

namespace EzCondo.BackEndServer.Data.Entities.SchemaPayment
{
	public class PaymentProviderConfig : IAuditableEntity
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

		public ICollection<PaymentOrder> PaymentOrders { get; set; } = new List<PaymentOrder>();
	}
}
