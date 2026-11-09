using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Enums;
using NpgsqlTypes;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;


namespace EzCondo.BackEndServer.Data.Entities.SchemaTenant
{
	public class Contract : IAuditableEntity, ISoftDeletableEntity
	{
		public Guid Id { get; set; }
		public string ContractNo { get; set; } = string.Empty;
		public string? ContractUrl { get; set; }
		public DateTimeOffset StartsAt { get; set; }
		public DateTimeOffset EndsAt { get; set; }
		public Guid TenantId { get; set; }
		public Guid ImportedBy { get; set; }
		public Guid ApartmentId { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }

		public Tenant Tenant { get; set; } = null!;
		public User StaffImported { get; set; } = null!;
		public Apartment Apartment { get; set; } = null!;
	}
}
