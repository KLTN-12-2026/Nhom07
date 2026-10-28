using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class ResidentComplaintConfiguration()
		: EntityConfigurationBase<ResidentComplaint>("resident_complaints", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<ResidentComplaint> b)
		{
			ConfigureUuidPrimaryKey(b);

			b.Property(x => x.Title)
				.HasMaxLength(200)
				.IsRequired();

			b.Property(x => x.Message)
				.IsRequired();

			b.Property(x => x.Status)
				.HasColumnType("tenant.resident_complaint_status")
				.HasDefaultValueSql("'assigned'::tenant.resident_complaint_status")
				.IsRequired();

			b.Property(x => x.ResolvedAt)
				.HasColumnType("timestamptz");

			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);
			ConfigureIsDeleted(b);
			ConfigureDeletedAt(b);

			b.Property(x => x.TenantId)
				.IsRequired();

			b.Property(x => x.ApartmentId)
				.IsRequired();

			b.Property(x => x.CreatedBy)
				.IsRequired();

			b.Property(x => x.CreatedBy)
				.IsRequired();

			b.Property(x => x.ReceivedBy)
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.ResidentComplaints)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.Apartment)
				.WithMany()
				.HasForeignKey(x => new {x.TenantId, x.ApartmentId })
				.HasPrincipalKey(x => new {x.TenantId, x.Id})
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.CreatedByUser)
				.WithMany(x => x.CreatedByUser)
				.HasForeignKey(x => new {x.TenantId, x.CreatedBy })
				.HasPrincipalKey(x => new {x.TenantId, x.Id})
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.ReceivedByUser)
				.WithMany(x => x.ReceivedByUser)
				.HasForeignKey(x => new { x.TenantId, x.ReceivedBy })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasIndex(x => new { x.Id, x.TenantId }).IsUnique();

			b.HasAlternateKey(x => new { x.TenantId, x.Id });
		}
	}
}
