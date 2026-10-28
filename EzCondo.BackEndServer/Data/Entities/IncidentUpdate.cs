using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class IncidentUpdate
	{
		public Guid Id { get; set; }
		public SystemIncidentStatus Status { get; set; }
		public string Message { get; set; }
		public DateTimeOffset CreatedAt { get; set; }

		public Guid IncidentId { get; set; }
		public SystemIncident SystemIncident { get; set; }
		public Guid AuthorId { get; set; }
		public IdentityUser User { get; set; }
	}
}
