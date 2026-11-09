using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaTenant
{
	internal sealed class ContractConfiguration()
		: EntityConfigurationBase<Contract>("contracts", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<Contract> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureIsDeleted(b);
			ConfigureDeletedAt(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);

			b.Property(x => x.ContractNo).HasMaxLength(80).IsRequired();
			b.Property(x => x.ContractUrl).HasColumnType("text");
			b.Property(x => x.StartsAt).HasColumnType("timestamptz").IsRequired();
			b.Property(x => x.EndsAt).HasColumnType("timestamptz").IsRequired();
			b.Property(x => x.TenantId).IsRequired();

			b.ToTable(t => t.HasCheckConstraint(
				"ck_contracts_period",
				"ends_at > starts_at"
			));

			b.HasOne(x => x.Tenant)
				.WithMany(t => t.Contracts)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.Apartment)
				.WithMany(x => x.Contracts)
				.HasForeignKey(x => new { x.TenantId, x.ApartmentId})
				.HasPrincipalKey(x => new {x.TenantId, x.Id})
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.StaffImported)
				.WithMany(x => x.Contracts)
				.HasForeignKey(x => x.ImportedBy)
				.OnDelete(DeleteBehavior.NoAction);

			b.HasIndex(x => new { x.TenantId, x.ContractNo })
				.IsUnique();
		}
	}
}
