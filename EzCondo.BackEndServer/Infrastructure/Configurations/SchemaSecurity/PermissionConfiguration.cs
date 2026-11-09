using EzCondo.BackEndServer.Data.Entities.SchemaSecurity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaSecurity
{
	internal sealed class PermissionConfiguration()
		: EntityConfigurationBase<Permission>("permissions", "security")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<Permission> b)
		{
			b.HasKey(x => new { x.Scope, x.Code });

			b.Property(x => x.Scope)
				.HasColumnType("security.role_scope_enum")
				.IsRequired();

			b.Property(x => x.Code)
				.HasMaxLength(120)
				.IsRequired();

			b.Property(x => x.Description)
				.HasColumnType("text")
				.IsRequired();

			b.ToTable(t => t.HasCheckConstraint(
				"ck_permissions_code_scope",
				"(scope = 'platform' AND code LIKE 'platform.%') " +
				"OR (scope = 'tenant' AND code LIKE 'tenant.%')"));
		}
	}
}
