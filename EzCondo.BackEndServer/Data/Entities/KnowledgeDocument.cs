using EzCondo.BackEndServer.Data.Enums;
using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class KnowledgeDocument : ISoftDeletableEntity, IAuditableEntity
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
		public DocumentTypeEnum Type { get; set; }
		public DocumentStatusEnum Status { get; set; }
		public NpgsqlRange<DateOnly>? EffectivePeriod { get; set; }
		public string Content { get; set; }
		public string? SourceUrl { get; set; }
		public NpgsqlTsVector SearchVector { get; private set; }
		public DateTimeOffset? PublishedAt { get; set; }
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }
		public Guid CreatedBy { get; set; }
		public IdentityUser User { get; set; }
	}
}
