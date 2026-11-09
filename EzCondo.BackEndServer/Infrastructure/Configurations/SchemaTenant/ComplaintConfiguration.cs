using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaTenant
{
	internal sealed class ComplaintConfiguration()
		: EntityConfigurationBase<Complaint>("complaints", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<Complaint> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);
			ConfigureIsDeleted(b);
			ConfigureDeletedAt(b);

			b.Property(x => x.Title)
				.HasMaxLength(200)
				.IsRequired();

			b.Property(x => x.Message)
				.HasColumnType("text")
				.IsRequired();

			b.Property(x => x.ResolvedAt)
				.HasColumnType("timestamptz");

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.Complaints)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.Resident)
				.WithMany(x => x.ComplaintResident)
				.HasForeignKey(x => x.CreatedByResident)
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.Staff)
				.WithMany(x => x.ComplaintStaff)
				.HasForeignKey(x => x.ReceivedByStaff)
				.OnDelete(DeleteBehavior.NoAction);

		}
	}
}
