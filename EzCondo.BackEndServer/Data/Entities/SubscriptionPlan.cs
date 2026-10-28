using System.Text.Json;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class SubscriptionPlan : ISoftDeletableEntity, IAuditableEntity
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string? Description { get; set; }
		public int BillingCycleMonths { get; set; }
		public Decimal Price { get; set; }
		public string FeatureLimits { get; set; }
		public bool IsAvailable { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }

		public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
	}
}
