using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class TenantRolePermissionConfiguration()
		: EntityConfigurationBase<TenantRolePermission>("tenant_role_permission", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<TenantRolePermission> b)
		{
			b.HasKey(x => new { x.RoleId, x.PermissionCode });

			b.Property(x => x.PermissionCode)
				.HasMaxLength(100)
				.IsRequired();

			b.HasOne(x => x.Role)
				.WithMany()
				.HasForeignKey(x => x.RoleId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.TenantPermission)
				.WithMany()
				.HasForeignKey(x => x.PermissionCode)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.Tenant)
				.WithMany()
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
