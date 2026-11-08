using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaPayment
{
	public class ProviderAccount : IAuditableEntity //Kênh PAYOS
	{
		public Guid Id { get; set; }
		public Guid? TenantId { get; set; }
		public Guid WebhookKey { get; set; }
		public string Code { get; set; }
		public string Provier { get; set; }
		public RoleScopeEnum OwnerScope { get; set; }
		public string? MerchantCode { get; set; }
		public string? PaymentCredentialsRef { get; set; }
		public string? PayoutCredentialsRef { get; set; }
		public bool SupportsCollection { get; set; }
		public bool SupportsPayout { get; set; }
		public string Settings { get; set; } 
		public bool IsActive { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public Tenant Tenant { get; set; }

		public ICollection<PaymentRequest> PaymentRequests { get; set; } = new List<PaymentRequest>();
		public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
		public ICollection<PaymentWebhookEvent> PaymentWebhookEvents { get; set; } = new List<PaymentWebhookEvent>();

	}
}
