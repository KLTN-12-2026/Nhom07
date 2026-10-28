using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
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

			b.Property(x => x.Slug)
				.HasColumnType("citext")
				.IsRequired();

			b.Property(x => x.LegalName)
				.HasMaxLength(255)
				.IsRequired();

			b.Property(x => x.TaxCode)
				.HasMaxLength(30);

			b.Property(x => x.Contact)
				.IsRequired();

			b.Property(x => x.Status)
				.HasColumnType("platform.tenant_status_enum")
				.HasDefaultValueSql("'trial'::platform.tenant_status_enum")
				.IsRequired();

			b.Property(x => x.SuspensionReason)
				.HasColumnType("text");

			b.HasIndex(x => x.Code)
				.IsUnique();

			b.HasIndex(x => x.Slug)
				.IsUnique();

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"CK_tenants_slug_format",
					"slug ~ '^[a-z0-9]+(?:-[a-z0-9]+)*$'"
					);

				x.HasCheckConstraint(
					"CK_tenants_trial_dates",
					"trial_ends_at IS NULL OR trial_started_at IS NULL OR trial_ends_at > trial_started_at"
					);
			});
		}
	}
}
