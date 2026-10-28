using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class SystemIncident : ISoftDeletableEntity, IAuditableEntity
	{
		public Guid Id { get; set; }
		public string Code { get; set; }
		public string Title { get; set; }
		public string? Description { get; set; }
		public int Severity { get; set; } //Mức đọo nghiêm trọng
		public SystemIncidentStatus Status { get; set; }
		public DateTimeOffset? ResolvedAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }

		public Guid CreatedBy { get; set; }
		public IdentityUser User { get; set; }

		public ICollection<IncidentAffectedTenant> IncidentAffectedTenants { get; set; } = new List<IncidentAffectedTenant>();
		public ICollection<IncidentUpdate> IncidentUpdates { get; set; } = new List<IncidentUpdate>();
	}
}
