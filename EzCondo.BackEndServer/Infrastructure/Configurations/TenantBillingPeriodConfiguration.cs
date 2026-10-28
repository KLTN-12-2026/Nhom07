using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class TenantBillingPeriodConfiguration()
		: EntityConfigurationBase<TenantBillingPeriod>("tenant_billing_periods", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<TenantBillingPeriod> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);

			b.Property(x => x.Name)
				.HasMaxLength(100)
				.IsRequired();

			b.Property(x => x.Period)
				.HasColumnType("daterange")
				.IsRequired();

			b.Property(x => x.DueAt)
				.IsRequired();

			b.Property(x => x.Status)
				.HasColumnType("tenant.billing_period_status")
				.HasDefaultValueSql("'draft'::tenant.billing_period_status")
				.IsRequired();

			b.Property(x => x.TenantId)
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.TenantBillingPeriods)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.User)
				.WithMany(x => x.TenantBillingPeriods)
				.HasForeignKey(x => new {x.TenantId, x.CreatedBy })
				.HasPrincipalKey(x => new {x.TenantId, x.Id})
				.OnDelete(DeleteBehavior.NoAction);

			b.HasIndex(x => new { x.TenantId, x.Id })
				.IsUnique();

			b.ToTable(x => x.HasCheckConstraint(
				"CK_billing_periods_period_not_empty",
				"NOT isempty(period)"
			));

			b.HasAlternateKey(x => new { x.TenantId, x.Id });
		}
	}
}
