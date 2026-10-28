using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class TenantAiChatSessionConfiguration()
		: EntityConfigurationBase<TenantAiChatSession>("tenant_ai_chat_sessions", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<TenantAiChatSession> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);

			b.Property(x => x.LastMessageAt)
				.HasDefaultValueSql("now()")
				.IsRequired();

			b.Property(x => x.TenantId)
				.IsRequired();

			b.Property(x => x.UserId)
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany()
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.User)
				.WithMany()
				.HasForeignKey(x => x.UserId)
				.OnDelete(DeleteBehavior.NoAction);

			b.HasAlternateKey(x => new { x.TenantId, x.Id });
		}
	}
}
