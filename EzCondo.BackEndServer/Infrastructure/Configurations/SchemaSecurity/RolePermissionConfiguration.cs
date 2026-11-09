using EzCondo.BackEndServer.Data.Entities.SchemaSecurity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaSecurity
{
	internal sealed class RolePermissionConfiguration()
		: EntityConfigurationBase<RolePermission>("role_permissions", "security")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<RolePermission> b)
		{
			b.HasKey(x => new { x.Scope, x.RoleId, x.PermissionCode });

			b.Property(x => x.Scope)
				.HasColumnType("security.role_scope_enum")
				.IsRequired();

			b.Property(x => x.PermissionCode)
				.HasMaxLength(120)
				.IsRequired();

			b.HasOne(x => x.Role)
				.WithMany(x => x.RolePermissions)
				.HasForeignKey(x => new { x.Scope, x.RoleId })
				.HasPrincipalKey(x => new { x.Scope, x.Id })
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.Permission)
				.WithMany(x => x.RolePermissions)
				.HasForeignKey(x => new { x.Scope, x.PermissionCode })
				.HasPrincipalKey(x => new { x.Scope, x.Code })
				.OnDelete(DeleteBehavior.Restrict);

			b.HasIndex(x => new { x.Scope, x.PermissionCode, x.RoleId })
				.HasDatabaseName("ix_role_permissions_permission");
		}
	}
}
