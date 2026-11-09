using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaTenant
{
	public class Complaint : ISoftDeletableEntity, IAuditableEntity
	{
		public Guid Id { get; set; }
		public string Title { get; set; } = null!;
		public string Message { get; set; } = null!;
		public DateTimeOffset? ResolvedAt { get; set; }

		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public Guid TenantId { get; set; }
		public Guid CreatedByResident { get; set; }
		public Guid? ReceivedByStaff { get; set; }

		public Tenant Tenant { get; set; } = null!;
		public User Resident { get; set; } = null!;
		public User? Staff { get; set; }
	}
}
