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
			b.HasKey(x => new { x.RoleId, x.PermissionCode });

			b.Property(x => x.PermissionCode)
				.HasMaxLength(100)
				.IsRequired();

			b.HasOne(x => x.Role)
				.WithMany(x => x.RolePermissions)
				.HasForeignKey(x => new {x.RoleId, x.Scope})
				.HasPrincipalKey(x => new {x.Id, x.RoleScope})
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.Permission)
				.WithMany(x => x.RolePermissions)
				.HasForeignKey(x => new { x.PermissionCode , x.Scope})
				.HasPrincipalKey(x => new {x.Code, x.Scope})
				.OnDelete(DeleteBehavior.Cascade);

			b.HasIndex(x => new { x.Scope, x.PermissionCode, x.RoleId })
				.HasDatabaseName("ix_role_permissions_permission");
		}
	}
}
