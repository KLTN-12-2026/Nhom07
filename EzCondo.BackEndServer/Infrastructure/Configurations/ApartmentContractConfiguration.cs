using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class ApartmentContractConfiguration()
		: EntityConfigurationBase<ApartmentContract>("apartment_contracts", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<ApartmentContract> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureIsDeleted(b);
			ConfigureDeletedAt(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);

			b.Property(x => x.ContractNo).HasMaxLength(80).IsRequired();
			b.Property(x => x.ContractPeriod).HasColumnType("daterange").IsRequired();
			b.Property(x => x.SignedAt).HasColumnType("date");
			b.Property(x => x.Status).HasColumnType("tenant.tenant_contract_status").HasDefaultValueSql("'active'::tenant.tenant_contract_status");
			b.Property(x => x.DocumentUrl).HasColumnType("text");
			b.Property(x => x.TenantId).IsRequired();
			b.Property(x => x.ApartmentId).IsRequired();
			b.Property(x => x.ResidentId).IsRequired();

			b.HasIndex(x => new { x.TenantId, x.ContractNo }).IsUnique();
			b.HasIndex(x => new { x.TenantId, x.Id }).IsUnique();
			b.HasIndex(x => x.ContractPeriod).HasMethod("gist");

			b.ToTable(t => t.HasCheckConstraint(
				"CK_apartment_contracts_contract_period",
				"NOT isempty(contract_period)" //Hợp đồng chỉ hợp lệ khi có thời hạn
			));

			b.HasOne(x => x.Tenant)
				.WithMany(t => t.ApartmentContracts)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.Apartment)
				.WithMany()
				.HasForeignKey(x => new { x.TenantId, x.ApartmentId})
				.HasPrincipalKey(x => new {x.TenantId, x.Id})
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.Resident)
				.WithMany()
				.HasForeignKey(x => new {x.TenantId, x.ResidentId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);
		}
	}
}
