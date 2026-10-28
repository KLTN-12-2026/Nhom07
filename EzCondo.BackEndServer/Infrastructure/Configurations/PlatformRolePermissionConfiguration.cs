using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class PlatformRolePermissionConfiguration()
		: EntityConfigurationBase<PlatformRolePermission>("platform_role_permissions", "platform")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<PlatformRolePermission> b)
		{
			b.HasKey(x => new { x.RoleId, x.PermissionCode });

			b.Property(x => x.PermissionCode)
				.HasMaxLength(100)
				.IsRequired();

			b.HasOne(x => x.PlatformRole)
				.WithMany()
				.HasForeignKey(x => x.RoleId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.PlatformPermission)
				.WithMany()
				.HasForeignKey(x => x.PermissionCode)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
