using EzCondo.BackEndServer.Data.Entities.SchemaSecurity;
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

			b.Property(x => x.Scope)
				.HasColumnType("security.role_scope_enum")
				.IsRequired();

			b.Property(x => x.GrantedAt)
				.HasColumnType("timestamptz")
				.IsRequired();

			b.Property(x => x.RevokedAt)
				.HasColumnType("timestamptz");

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.UserRoles)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.User)
				.WithMany(x => x.UserRoles)
				.HasForeignKey(x => x.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.GrantedByUser)
				.WithMany(x => x.GrantedUserRoles)
				.HasForeignKey(x => x.GrantedByUserId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.Role)
				.WithMany(x => x.UserRoles)
				.HasForeignKey(x => new { x.Scope, x.RoleId })
				.HasPrincipalKey(x => new { x.Scope, x.Id })
				.OnDelete(DeleteBehavior.Restrict);

			b.ToTable(t =>
			{
				t.HasCheckConstraint(
					"ck_user_roles_scope_tenant",
					"(scope = 'platform' AND tenant_id IS NULL) OR (scope = 'tenant' AND tenant_id IS NOT NULL)");

				t.HasCheckConstraint(
					"ck_user_roles_revoked_at",
					"revoked_at IS NULL OR revoked_at >= granted_at");
			});

			b.HasIndex(x => new { x.UserId, x.RoleId })
				.HasDatabaseName("ix_user_role_active")
				.IsUnique()
				.HasFilter("revoked_at IS NULL");

			b.HasIndex(x => x.UserId)
				.HasDatabaseName("ux_user_roles_platform_user_active")
				.IsUnique()
				.HasFilter("scope = 'platform' AND revoked_at IS NULL");

			b.HasIndex(x => new { x.TenantId, x.UserId })
				.HasDatabaseName("ix_user_roles_tenant_user_active")
				.HasFilter("scope = 'tenant' AND revoked_at IS NULL")
				.IncludeProperties(x => x.RoleId);
		}
	}
}
