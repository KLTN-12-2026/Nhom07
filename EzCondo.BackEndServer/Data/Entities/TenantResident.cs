using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class TenantResident : ISoftDeletableEntity, IAuditableEntity
	{
		public Guid Id { get; set; }
		public string FullName { get; set; }
		public string? Email { get; set; }
		public string? Phone { get; set; }
		public DateOnly? DateOfBirth { get; set; }
		public string? CitizenId { get; set; } //cccd
		public TenantResidentStatus Status { get; set; }
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }
		public Guid UserId { get; set; }
		public IdentityUser User { get; set; }

		public ICollection<ApartmentResident> ApartmentResidents { get; set; } = new List<ApartmentResident>();
	}
}
