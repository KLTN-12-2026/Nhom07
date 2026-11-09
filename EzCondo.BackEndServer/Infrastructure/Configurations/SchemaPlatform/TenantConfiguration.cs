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

			b.HasIndex(x => x.Code).IsUnique();
			b.HasIndex(x => x.TaxCode).IsUnique();
			b.HasIndex(x => x.Slug).IsUnique();

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"CK_tenants_slug_format",
					"slug ~ '^[a-z0-9]+(?:-[a-z0-9]+)*$'"
					);

				x.HasCheckConstraint(
					"ck_tenants_trial",
					"status <> 'trial' OR trial_ends_at IS NOT NULL");
			});
		}
	}
}
