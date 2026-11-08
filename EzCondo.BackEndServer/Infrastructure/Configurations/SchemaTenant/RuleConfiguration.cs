using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaTenant
{
	internal sealed class RuleConfiguration()
		: EntityConfigurationBase<Rule>("rules", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<Rule> b)
		{
			ConfigureUuidPrimaryKey(b);

			b.Property(x => x.Title)
				.HasMaxLength(500)
				.IsRequired();

			b.Property(x => x.TenantId)
				.IsRequired();

			b.Property(x => x.SourceUrl)
				.HasColumnType("text")
				.IsRequired();

			b.HasIndex(x => new { x.TenantId, x.Id, x.ImportedBy }).IsUnique();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.Rules)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.TenantOwnerImported)
				.WithMany(x => x.Rules)
				.HasForeignKey(x => new { x.TenantId, x.ImportedBy })
				.HasPrincipalKey(x => new {x.TenantId, x.Id})
				.OnDelete(DeleteBehavior.NoAction);
		}
	}
}
