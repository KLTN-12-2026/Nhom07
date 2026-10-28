using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class ResidentComplaint : ISoftDeletableEntity, IAuditableEntity
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
		public string Message { get; set; }
		public ResidentComplaintStatus Status { get; set; }
		public DateTimeOffset? ResolvedAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }
		public Guid ApartmentId { get; set; }
		public ApartmentResident Apartment { get; set; }
		public Guid CreatedBy { get; set; }
		public IdentityUser CreatedByUser { get; set; }
		public Guid ReceivedBy { get; set; }
		public IdentityUser ReceivedByUser { get; set; }
	}
}
