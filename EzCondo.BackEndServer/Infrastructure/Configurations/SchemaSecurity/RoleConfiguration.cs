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

			b.Property(x => x.Code)
				.HasColumnType("tenant_role_code_enum")
				.IsRequired();

			b.Property(x => x.Name)
				.HasMaxLength(150)
				.IsRequired();

			b.Property(x => x.RoleScope)
				.HasColumnType("role_scope_enum")
				.IsRequired();

			b.Property(x => x.HierarchyLevel)
				.HasColumnType("short")
				.IsRequired();

			b.Property(x => x.IsDeleted)
				.HasDefaultValue("false")
				.IsRequired();

			b.Property(x => x.IsSystem)
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.Roles)
				.HasForeignKey(x => x.TenantId)
				.OnDelete
				(DeleteBehavior.NoAction);

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"CK_roles_role_belong_to_each_scope",
					"(scope = 'platform' AND tenant_id IS NULL) OR (scope = 'tenant' AND tenant_id IS NOT NULL)");
			});

			b.HasIndex(x => new { x.TenantId, x.Code })
				.HasDatabaseName("ux_roles_tenant_code")
				.IsUnique()
				.HasFilter("scope = 'tenant'");

			b.HasIndex(x => new { x.Code })
				.HasDatabaseName("ux_roles_platform_code")
				.IsUnique()
				.HasFilter("scope = 'platform'");

			b.HasIndex(x => new { x.TenantId, x.Id });
			b.HasIndex(x => new { x.Id, x.RoleScope});
		}
	}
}
