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

			b.HasIndex(x => new { x.Code, x.TaxCode, x.Slug})
				.IsUnique();

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"CK_tenants_slug_format",
					"slug ~ '^[a-z0-9]+(?:-[a-z0-9]+)*$'"
					);
			});
		}
	}
}
