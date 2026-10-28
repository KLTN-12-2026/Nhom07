using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class Subscription : IAuditableEntity
	{
		public Guid Id { get; set; }
		public SubscriptionStatusEnum Status { get; set; }
		public DateTimeOffset StartsAt { get; set; }
		public DateTimeOffset EndsAt { get; set; }
		public bool AutoRenew { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }
		public DateTimeOffset? CancelledAt { get; set; }

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }
		public Guid PlanId { get; set; }
		public SubscriptionPlan SubscriptionPlan { get; set; }

		public ICollection<SubscriptionInvoice> SubscriptionInvoices { get; set; } = new List<SubscriptionInvoice>();
	}
}
