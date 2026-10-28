using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class SupportTicket : ISoftDeletableEntity, IAuditableEntity
	{
		public Guid Id { get; set; }
		public string Subject { get; set; }
		public SupportTicketStatus Status { get; set; }
		public DateTimeOffset? ResolvedAt { get; set; }
		public string Message { get; set; }
		public DateTimeOffset? DueAt { get; set; }
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }
		public Guid RequesterId { get; set; }
		public IdentityUser RequesterUser { get; set; }
		public Guid ReceiverId { get; set; }
		public IdentityUser ReceiverUser { get; set; }
	}
}
