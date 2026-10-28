using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class TenantPermissionConfiguration()
		: EntityConfigurationBase<TenantPermission>("tenant_permissions", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<TenantPermission> b)
		{
			b.HasKey(x => x.Code);

			b.Property(x => x.Code)
				.HasMaxLength(100);

			b.Property(x => x.Description)
				.HasColumnType("text")
				.IsRequired();
		}
	}
}
