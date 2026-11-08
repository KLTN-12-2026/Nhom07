using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaTenant
{
	internal sealed class InvitationConfiguration()
		: EntityConfigurationBase<Invitation>("tenant_invitations", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<Invitation> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureIsDeleted(b);
			ConfigureDeletedAt(b);

			b.Property(x => x.Email)
				.HasColumnType("citext")
				.IsRequired();

			b.Property(x => x.TokenHash)
				.HasColumnType("text")
				.IsRequired();

			b.Property(x => x.AcceptedAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.RevokedAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.ExpiresAt)
				.HasColumnType("timestamptz")
				.IsRequired();

			b.Property(x => x.Status)
				.HasColumnType("tenant.invitation_status_enum")
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.Invitations)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.Role)
				.WithMany(x => x.Invitations)
				.HasForeignKey(x => new {x.TenantId, x.RoleId })
				.HasPrincipalKey(x => new {x.TenantId, x.Id})
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.User)
				.WithMany(x => x.CreatedByUser)
				.HasForeignKey(x => new { x.TenantId, x.CreatedBy })
				.HasPrincipalKey(x => new {x.TenantId, x.Id})
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.TargetUser)
				.WithMany(x => x.TargetUser)
				.HasForeignKey(x => new { x.TenantId, x.TargetUserId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasIndex(x => x.TokenHash)
				.IsUnique();

			b.HasIndex(x => new { x.TenantId, x.Email, x.TokenHash }).IsUnique();

			b.HasIndex(x => new { x.TenantId, x.Email, x.RoleId })
				.HasDatabaseName("ix_invitations_active")
				.HasFilter("status = 'sent' AND is_deleted = false");

			b.HasIndex(x => new { x.Status, x.ExpiresAt })
				.HasDatabaseName("ix_invitations_expiring")
				.HasFilter("status = 'sent'");
		}
	}
}
