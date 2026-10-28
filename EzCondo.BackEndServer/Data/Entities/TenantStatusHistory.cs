using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class TenantStatusHistory
	{
		public Guid Id { get; set; }
		public TenantStatusEnum? OldStatus { get; set; }
		public TenantStatusEnum CurrentStatus { get; set; }
		public string? Reason { get; set; }
		public DateTimeOffset ChangedAt { get; set; }

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }
		public Guid ChangedBy { get; set; }
		public IdentityUser User { get; set; }
	}
}