using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaSecurity
{
	internal sealed class UserRoleConfiguration()
		: EntityConfigurationBase<UserRole>("user_roles", "security")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<UserRole> b)
		{
			ConfigureUuidPrimaryKey(b);

			b.Property(x => x.GrantedAt)
				.HasColumnType("Datetime")
				.IsRequired();

			b.Property(x => x.RevokedAt)
				.HasColumnType("Datetime");

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.UserRoles)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.NewUser)
				.WithMany(x => x.NewUsers)
				.HasForeignKey(x => new { x.TenantId, x.NewUserId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.UserGranted)
				.WithMany(x => x.UserGranted)
				.HasForeignKey(x => new { x.TenantId, x.GrantedBy })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.Role)
				.WithMany(x => x.UserRoles)
				.HasForeignKey(x => new { x.TenantId, x.RoleId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.Restrict);

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"CK_user_roles_revoked_at_le_granted_at",
					"revoked_at IS NULL OR revoked_at >= granted_at");
			});

			b.HasIndex(x => new { x.RoleId, x.NewUserId})
				.HasDatabaseName("ix_user_role_active")
				.IsUnique()
				.HasFilter("revoked_at IS NULL");

			b.HasIndex(x => x.NewUserId)
				.HasDatabaseName("ix_user_roles_user_active")
				.IncludeProperties(x => x.RoleId)
				.HasFilter("revoked_at IS NULL");
		}
	}
}
