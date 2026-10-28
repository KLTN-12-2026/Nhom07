using EzCondo.BackEndServer.Data.Enums;
using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class ApartmentContract : ISoftDeletableEntity, IAuditableEntity
	{
		public Guid Id { get; set; }
		public string ContractNo { get; set; } = string.Empty;
		public NpgsqlRange<DateOnly>? ContractPeriod { get; set; }
		public DateOnly? SignedAt { get; set; }
		public TenantContractStatus Status { get; set; }
		public string? DocumentUrl { get; set; }
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }
		public Guid ApartmentId { get; set; }
		public TenantApartment Apartment { get; set; }
		public Guid ResidentId { get; set; }
		public TenantResident Resident { get; set; }
	}
}
