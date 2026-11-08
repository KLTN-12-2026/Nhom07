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
			b.HasKey(x => x.Code);

			b.Property(x => x.Code)
				.HasMaxLength(100);

			b.Property(x => x.Description)
				.HasColumnType("text")
				.IsRequired();
		}
	}
}
