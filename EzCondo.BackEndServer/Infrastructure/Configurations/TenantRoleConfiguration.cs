using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class TenantRoleConfiguration()
		: EntityConfigurationBase<TenantRole>("tenant_roles", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<TenantRole> b)
		{
			ConfigureCreatedAt(b);

			b.HasKey(x => x.Role);

			b.Property(x => x.Code)
				.HasMaxLength(80)
				.IsRequired();

			b.HasIndex(x => new { x.Code, x.TenantId})
				.IsUnique();

			b.Property(x => x.Name)
				.HasMaxLength(150)
				.IsRequired();

			b.Property(x => x.HierarchyLevel)
				.HasColumnType("smallint")
				.IsRequired();

			b.Property(x => x.IsSystem)
				.HasDefaultValue(false)
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.TenantRoles)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);
			
			b.ToTable(t => t.HasCheckConstraint(
				"tenant_roles_hierarchy_check", 
				"hierarchy_level BETWEEN 0 AND 100"
			));
		}
	}
}
