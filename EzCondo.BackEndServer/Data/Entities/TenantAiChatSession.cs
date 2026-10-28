namespace EzCondo.BackEndServer.Data.Entities
{
	public class TenantAiChatSession
	{
		public Guid Id { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset LastMessageAt { get; set; }

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }
		public Guid UserId { get; set; }
		public IdentityUser User { get; set; }

		public ICollection<TenantAiChatMessage> TenantAiChatMessages { get; set; } = new List<TenantAiChatMessage>();
	}
}
