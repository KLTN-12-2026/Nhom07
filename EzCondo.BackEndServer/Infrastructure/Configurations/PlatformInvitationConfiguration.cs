using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class PlatformInvitationConfiguration()
		: EntityConfigurationBase<PlatformInvitation>("platform_invitations", "platform")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<PlatformInvitation> b)
		{
			ConfigureUuidPrimaryKey(b);

			b.Property(x => x.Email)
				.HasColumnType("citext")
				.IsRequired();

			b.Property(x => x.TokenHash)
				.HasColumnType("text")
				.IsRequired();

			b.Property(x => x.ExpiresAt)
				.HasColumnType("timestamptz")
				.IsRequired();

			b.Property(x => x.AcceptedAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.RevokedAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.PlatformRoleId)
				.IsRequired();

			b.Property(x => x.InvitedBy)
				.IsRequired();

			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);
			ConfigureIsDeleted(b);
			ConfigureDeletedAt(b);

			b.HasIndex(x => x.TokenHash)
				.IsUnique();

			b.HasOne(x => x.PlatformRole)
				.WithMany()
				.HasForeignKey(x => x.PlatformRoleId)
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.User)
				.WithMany(x => x.PlatformInvitations)
				.HasForeignKey(x => x.InvitedBy)
				.OnDelete(DeleteBehavior.NoAction);

			b.HasIndex(x => new { x.Email, x.PlatformRoleId })
				.HasDatabaseName("platform_invitations_idx")
				.IsUnique()
				.HasFilter("accepted_at IS NULL AND revoked_at IS NULL");
		}
	}
}
