using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class TenantInvitationConfiguration()
		: EntityConfigurationBase<TenantInvitation>("tenant_invitations", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<TenantInvitation> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);
			ConfigureIsDeleted(b);
			ConfigureDeletedAt(b);

			b.Property(x => x.Email)
				.HasColumnType("citext")
				.IsRequired();

			b.Property(x => x.TokenHash)
				.HasColumnType("text")
				.IsRequired();

			b.Property(x => x.ExpiresAt)
				.IsRequired();

			b.Property(x => x.TenantId)
				.IsRequired();

			b.Property(x => x.TenantRoleId)
				.IsRequired();

			b.Property(x => x.InvitedBy)
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.TenantInvitations)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.TenantRole)
				.WithMany()
				.HasForeignKey(x => x.TenantRoleId)
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.User)
				.WithMany(x => x.TenantInvitations)
				.HasForeignKey(x => new {x.TenantId, x.InvitedBy })
				.HasPrincipalKey(x => new {x.TenantId, x.Id})
				.OnDelete(DeleteBehavior.NoAction);

			b.HasIndex(x => x.TokenHash)
				.IsUnique();

			b.HasIndex(x => new { x.TenantId, x.Id }).IsUnique();

			b.ToTable(x => x.HasCheckConstraint(
				"CK_tenant_invitations_expires_at_le_created_at",
				"expires_at > created_at"
				));

			b.HasAlternateKey(x => new { x.TenantId, x.Id });

			b.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
		}
	}
}
