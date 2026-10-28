using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class PlatformRoleConfiguration()
		: EntityConfigurationBase<PlatformRole>("platform_roles", "platform")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<PlatformRole> b)
		{
			ConfigureUuidPrimaryKey(b);

			b.Property(x => x.Code)
				.HasMaxLength(80)
				.IsRequired();

			b.Property(x => x.Name)
				.HasMaxLength(150)
				.IsRequired();

			b.Property(x => x.IsSystem)
				.HasDefaultValue(true)
				.IsRequired();

			b.HasIndex(x => x.Code)
				.IsUnique();
		}
	}
}
