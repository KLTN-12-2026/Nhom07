using System.Text.Json;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class SubscriptionEvent : IAuditableEntity //Log sự kiện đổi Subscription của tenant
	{
		public Guid Id { get; set; }
		public string EventType { get; set; }
		public string Data { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public Guid SubscriptionId { get; set; }
		public Subscription Subscription { get; set; }
	}
}
