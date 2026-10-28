using EzCondo.BackEndServer.Data.Enums;
using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class ApartmentResident
	{
		public Guid Id { get; set; }
		public NpgsqlRange<DateOnly> OccupancyPeriod { get; set; }
		public DateTimeOffset CreatedAt { get; set; }

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }
		public Guid ApartmentId { get; set; }
		public TenantApartment Apartment { get; set; }
		public Guid ResidentId { get; set; }
		public TenantResident Resident { get; set; }

		public ICollection<ResidentComplaint> ResidentComplaints { get; set; } = new List<ResidentComplaint>();
	}
}
