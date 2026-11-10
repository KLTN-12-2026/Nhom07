using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaPlatform
{
	internal sealed class TenantConfiguration()
		: EntityConfigurationBase<Tenant>("tenants", "platform")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<Tenant> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);

			b.Property(x => x.Code)
				.HasMaxLength(30)
				.IsRequired();

			b.Property(x => x.Address)
				.HasColumnType("text")
				.IsRequired();

			b.Property(x => x.Slug)
				.HasColumnType("citext")
				.IsRequired();

			b.Property(x => x.LegalName)
				.HasMaxLength(255)
				.IsRequired();

			b.Property(x => x.TaxCode)
				.HasMaxLength(30)
				.IsRequired();

			b.Property(x => x.Status)
				.HasColumnType("platform.tenant_status_enum")
				.IsRequired();

			b.Property(x => x.TrialEndsAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.SuspensionEffectiveAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.SuspensionReasonCode)
				.HasMaxLength(40);

			b.Property(x => x.SuspensionReason)
				.HasMaxLength(1000);

			b.Property(x => x.SuspensionNoticeSentAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.SuspendedAt)
				.HasColumnType("timestamptz");

			b.HasIndex(x => x.SuspensionEffectiveAt)
				.HasDatabaseName("ix_tenants_pending_suspension")
				.HasFilter(
					"suspension_effective_at IS NOT NULL " +
					"AND suspended_at IS NULL");

			b.HasIndex(x => x.Code).IsUnique();
			b.HasIndex(x => x.TaxCode).IsUnique();
			b.HasIndex(x => x.Slug).IsUnique();

			b.HasOne<User>()
				.WithMany()
				.HasForeignKey(x => x.SuspensionRequestedByAdminId)
				.OnDelete(DeleteBehavior.Restrict);

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"CK_tenants_slug_format",
					"slug ~ '^[a-z0-9]+(?:-[a-z0-9]+)*$'"
					);

				x.HasCheckConstraint(
					"ck_tenants_trial",
					"status <> 'trial' OR trial_ends_at IS NOT NULL");

				x.HasCheckConstraint(
					"ck_tenants_suspension_reason_code",
					"suspension_reason_code IS NULL " +
					"OR suspension_reason_code IN " +
					"('owner_request', 'policy_violation')");

				x.HasCheckConstraint(
					"ck_tenants_suspension_notice_period",
					"suspension_notice_sent_at IS NULL " +
					"OR suspension_effective_at >= " +
					"suspension_notice_sent_at + INTERVAL '12 hours'");

				x.HasCheckConstraint(
					"ck_tenants_suspended_at",
					"status <> 'suspended' OR suspended_at IS NOT NULL");
			});


		}
	}
}
