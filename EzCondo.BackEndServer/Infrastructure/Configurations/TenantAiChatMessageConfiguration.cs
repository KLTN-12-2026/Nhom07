using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class TenantAiChatMessageConfiguration()
		: EntityConfigurationBase<TenantAiChatMessage>("tenant_ai_chat_messages", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<TenantAiChatMessage> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);

			b.Property(x => x.Role)
				.HasColumnType("tenant.ai_role_enum")
				.IsRequired();

			b.Property(x => x.Content)
				.HasColumnType("text")
				.IsRequired();

			b.Property(x => x.Citations)
				.HasColumnType("jsonb")
				.HasDefaultValueSql("'{}'::jsonb")
				.IsRequired();

			b.Property(x => x.Model)
				.HasMaxLength(100);

			b.Property(x => x.SessionId)
				.IsRequired();

			b.HasOne(x => x.TenantAiChatSession)
				.WithMany(x => x.TenantAiChatMessages)
				.HasForeignKey(x => x.SessionId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.Tenant)
				.WithMany()
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
