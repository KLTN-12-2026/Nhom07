using EzCondo.BackEndServer.Data.Entities.SchemaSecurity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaSecurity
{
	internal sealed class RoleConfiguration()
		: EntityConfigurationBase<Role>("roles", "security")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<Role> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureIsDeleted(b);

			b.Property(x => x.Scope)
				.HasColumnType("security.role_scope_enum")
				.IsRequired();

			b.Property(x => x.Code)
				.HasMaxLength(80)
				.IsRequired();

			b.Property(x => x.Name)
				.HasMaxLength(150)
				.IsRequired();

			b.Property(x => x.IsSystem)
				.HasDefaultValue(false)
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.Roles)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasAlternateKey(x => new { x.Scope, x.Id });

			b.ToTable(t =>
			{
				t.HasCheckConstraint(
					"ck_roles_owner_scope",
					"(scope = 'platform' AND tenant_id IS NULL) OR (scope = 'tenant' AND tenant_id IS NOT NULL)");

				t.HasCheckConstraint(
					"ck_roles_platform_code",
					"scope <> 'platform' OR code = 'PLATFORM_ADMIN'");
			});

			b.HasIndex(x => new { x.TenantId, x.Code })
				.HasDatabaseName("ux_roles_tenant_code")
				.IsUnique()
				.HasFilter("scope = 'tenant' AND is_deleted = false");

			b.HasIndex(x => x.Code)
				.HasDatabaseName("ux_roles_platform_code")
				.IsUnique()
				.HasFilter("scope = 'platform' AND is_deleted = false");
		}
	}
}
