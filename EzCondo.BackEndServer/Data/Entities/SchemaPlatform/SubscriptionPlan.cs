using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using System.Text.Json;

namespace EzCondo.BackEndServer.Data.Entities.SchemaPlatform
{
	public class SubscriptionPlan : ISoftDeletableEntity, IAuditableEntity
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = null!;
		public string? Description { get; set; }
		public int BillingCycleMonths { get; set; }
		public Decimal Price { get; set; }
		public string FeatureLimits { get; set; } = null!;
		public bool IsAvailable { get; set; }
		public Guid CreatedByAdmin { get; set; }
		public Guid? UpdatedByAdmin { get; set; }
		public Guid? DeletedByAdmin { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }

		public User AdminCreated { get; set; } = null!;
		public User? AdminUpdated { get; set; }
		public User? AdminDeleted { get; set; }

		public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
	}
}
