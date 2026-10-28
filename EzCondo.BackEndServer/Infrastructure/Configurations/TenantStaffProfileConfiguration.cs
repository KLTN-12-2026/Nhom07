using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class TenantStaffProfileConfiguration()
		: EntityConfigurationBase<TenantStaffProfile>("tenant_staff_profiles", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<TenantStaffProfile> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);
			ConfigureDeletedAt(b);
			ConfigureIsDeleted(b);

			//b.Property(x => x.DateOfBirth)
			//	.HasColumnType("date");

			b.Property(x => x.BankName)
				.HasMaxLength(100)
				.IsRequired();

			b.Property(x => x.BankAccountNumber)
				.HasMaxLength(50)
				.IsRequired();

			b.HasIndex(x => x.BankAccountNumber)
				.IsUnique();

			b.HasOne(x => x.Tenant)
				.WithMany()
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.User)
				.WithMany()
				.HasForeignKey(x => new { x.TenantId, x.UserId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.Cascade);

			b.HasIndex(x => new { x.TenantId, x.Id }).IsUnique();

			b.HasAlternateKey(x => new { x.TenantId, x.Id });
		}
	}
}
