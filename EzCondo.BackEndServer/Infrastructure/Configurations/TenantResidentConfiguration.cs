using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class TenantResidentConfiguration()
		: EntityConfigurationBase<TenantResident>("tenant_residents", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<TenantResident> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);
			ConfigureDeletedAt(b);
			ConfigureIsDeleted(b);

			b.Property(x => x.FullName)
				.HasMaxLength(200)
				.IsRequired();

			b.Property(x => x.Email)
				.HasColumnType("citext");

			b.Property(x => x.Phone)
				.HasMaxLength(30);

			b.Property(x => x.DateOfBirth)
				.IsRequired();

			b.Property(x => x.Status)
				.HasColumnType("tenant.tenant_resident_status")
				.HasDefaultValueSql("'invited'::tenant.tenant_resident_status")
				.IsRequired();

			b.Property(x => x.CitizenId)
				.HasMaxLength(50);

			b.Property(x => x.TenantId)
				.IsRequired();

			b.Property(x => x.UserId)
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.TenantResidents)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.User)
				.WithMany(x => x.TenantResidents)
				.HasForeignKey(x => new { x.TenantId, x.UserId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasIndex(x => new { x.TenantId, x.CitizenId })
				.IsUnique();

			b.HasIndex(x => new { x.TenantId, x.Id })
				.IsUnique();

			b.HasIndex(x => new { x.TenantId, x.UserId }).IsUnique();

			b.HasAlternateKey(x => new { x.TenantId, x.Id });
		}
	}
}
