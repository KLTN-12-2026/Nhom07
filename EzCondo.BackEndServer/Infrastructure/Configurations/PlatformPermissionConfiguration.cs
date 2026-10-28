using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class PlatformPermissionConfiguration()
		: EntityConfigurationBase<PlatformPermission>("platform_permissions", "platform")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<PlatformPermission> b)
		{
			b.HasKey(x => x.Code);

			b.Property(x => x.Code)
				.HasMaxLength(100)
				.ValueGeneratedNever()
				.IsRequired();

			b.Property(x => x.Description)
				.HasColumnType("text")
				.IsRequired();
		}
	}
}
