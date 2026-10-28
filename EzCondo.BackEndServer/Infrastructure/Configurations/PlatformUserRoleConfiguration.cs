using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class PlatformUserRoleConfiguration()
		: EntityConfigurationBase<PlatformUserRole>("platform_user_role", "platform")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<PlatformUserRole> b)
		{
			b.HasKey(x => new { x.UserId, x.RoleId });

			b.Property(x => x.GrantedAt)
				.HasColumnType("timestamptz")
				.HasDefaultValueSql("now()")
				.IsRequired();

			b.HasOne(x => x.User)
				.WithMany()
				.HasForeignKey(x => x.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.PlatformRole)
				.WithMany()
				.HasForeignKey(x => x.RoleId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.GrantedByUser)
				.WithMany(x => x.PlatformUserRoles)
				.HasForeignKey(x => x.GrantedBy)
				.OnDelete(DeleteBehavior.SetNull);
		}
	}
}
