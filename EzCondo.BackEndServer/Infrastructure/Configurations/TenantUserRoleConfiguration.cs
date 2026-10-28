using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class TenantUserRoleConfiguration()
		: EntityConfigurationBase<TenantUserRole>("tenant_user_roles", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<TenantUserRole> b)
		{
			ConfigureUuidPrimaryKey(b);

			b.Property(x => x.GrantedAt)
				.IsRequired();

			b.HasOne(x => x.User)
				.WithMany(x => x.TenantUserRoles)
				.HasForeignKey(x => x.UserId)
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.GrantedUser)
				.WithMany()
				.HasForeignKey(x => x.GrantedBy)
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.Role)
				.WithMany()
				.HasForeignKey(x => x.RoleId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
