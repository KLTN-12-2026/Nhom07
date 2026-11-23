using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaTenant
{
	internal sealed class InvitationConfiguration()
		: EntityConfigurationBase<Invitation>("invitations", "tenant")
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

			b.Property(x => x.EmailSentAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.LastEmailAttemptAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.EmailSendAttemptCount)
				.HasDefaultValue(0)
				.IsRequired();

			b.Property(x => x.LastEmailErrorCode)
				.HasMaxLength(100);

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.Invitations)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.Role)
				.WithMany(x => x.Invitations)
				.HasForeignKey(x => x.RoleId)
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.User)
				.WithMany(x => x.CreatedByUser)
				.HasForeignKey(x => x.CreatedBy)
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.TargetUser)
				.WithMany(x => x.TargetUser)
				.HasForeignKey(x => x.TargetUserId)
				.OnDelete(DeleteBehavior.NoAction);

			b.HasIndex(x => x.TokenHash)
				.IsUnique();

			b.HasIndex(x => new { x.TenantId, x.Email, x.TokenHash }).IsUnique();

			b.HasIndex(x => new
			{
				x.TenantId,
				x.RoleId
			})
			.HasDatabaseName("ux_invitations_active_tenant_role")
			.IsUnique()
			.HasFilter(
				"status IN ('pending_delivery', 'sent') " +
				"AND is_deleted = false");

			b.HasIndex(x => new { x.Status, x.ExpiresAt })
				.HasDatabaseName("ix_invitations_expiring")
				.HasFilter(
					"status IN ('pending_delivery', 'sent')");

			b.ToTable(t =>
			{
				t.HasCheckConstraint(
					"ck_invitations_accepted",
					"status <> 'accepted' OR accepted_at IS NOT NULL");
				t.HasCheckConstraint(
					"ck_invitations_revoked",
					"status <> 'revoked' OR revoked_at IS NOT NULL");
				t.HasCheckConstraint(
					"ck_invitations_expiry",
					"expires_at > created_at");
				t.HasCheckConstraint(
					"ck_invitations_email_attempt_count",
					"email_send_attempt_count >= 0");

				t.HasCheckConstraint(
					"ck_invitations_sent_email",
					"status <> 'sent' OR email_sent_at IS NOT NULL");
			});
		}
	}
}
