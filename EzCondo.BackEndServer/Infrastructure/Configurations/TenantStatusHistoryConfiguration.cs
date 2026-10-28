using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class TenantStatusHistoryConfiguration()
		: EntityConfigurationBase<TenantStatusHistory>("tenant_status_histories", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<TenantStatusHistory> b)
		{
			ConfigureUuidPrimaryKey(b);

			b.Property(x => x.OldStatus)
				.HasColumnType("platform.tenant_status_enum");

			b.Property(x => x.CurrentStatus)
				.HasColumnType("platform.tenant_status_enum")
				.IsRequired();

			b.Property(x => x.Reason)
				.HasColumnType("text");

			b.Property(x => x.ChangedAt)
				.HasColumnType("timestamptz")
				.HasDefaultValueSql("now()")
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.TenantStatusHistories)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.User)
				.WithMany()
				.HasForeignKey(x => new { x.TenantId, x.ChangedBy })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasIndex(x => new { x.TenantId, x.Id }).IsUnique();
		}
	}
}
