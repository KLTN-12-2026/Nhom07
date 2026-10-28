using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class KnowledgeDocumentConfiguration()
		: EntityConfigurationBase<KnowledgeDocument>("knowledge_documents", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<KnowledgeDocument> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureIsDeleted(b);
			ConfigureDeletedAt(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);

			b.Property(x => x.Title)
				.HasMaxLength(500)
				.IsRequired();

			b.Property(x => x.Type)
				.HasColumnType("tenant.document_type_enum")
				.IsRequired();

			b.Property(x => x.Status)
				.HasColumnType("tenant.document_status_enum")
				.HasDefaultValueSql("'draft'::tenant.document_status_enum")
				.IsRequired();

			b.Property(x => x.EffectivePeriod)
				.HasColumnType("daterange");

			b.Property(x => x.Content)
				.HasColumnType("text")
				.IsRequired();

			b.Property(x => x.SourceUrl)
				.HasColumnType("text");

			b.Property(x => x.SearchVector)
				.HasColumnType("tsvector")
				.HasComputedColumnSql(
					"to_tsvector('simple', coalesce(title, '') || ' ' || coalesce(content, ''))",
					stored: true);

			b.Property(x => x.TenantId)
				.IsRequired();

			b.Property(x => x.CreatedBy)
				.IsRequired();

			b.HasIndex(x => x.SearchVector)
				.HasMethod("gin");

			b.HasIndex(x => x.EffectivePeriod)
				.HasMethod("gist");

			b.HasIndex(x => new { x.TenantId, x.PublishedAt })
				.HasDatabaseName("knowledga_documents_idx")
				.HasFilter("\"status\" = 'published'::tenant.document_status_enum AND \"is_deleted\" = false"); //Chỉ sử dụng doc đã đc công bố avf chưa bị xóa

			b.HasIndex(x => new { x.TenantId, x.Id }).IsUnique();

			b.ToTable(t =>
			{
				t.HasCheckConstraint(
					"CK_knowledge_documents_published_at",
					"status <> 'published'::tenant.document_status_enum OR published_at IS NOT NULL"); //Bắt buộc PublishedAt khi status=published
			});

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.KnowledgeDocuments)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.User)
				.WithMany(x => x.KnowledgeDocuments)
				.HasForeignKey(x => new {x.TenantId, x.CreatedBy })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);
		}
	}
}
