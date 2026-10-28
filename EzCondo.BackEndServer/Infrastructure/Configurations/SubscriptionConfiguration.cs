using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class SubscriptionConfiguration()
		: EntityConfigurationBase<Subscription>("subscriptions", "platform")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<Subscription> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);

			b.Property(x => x.Status)
				.HasColumnType("platform.subscription_status_enum")
				.HasDefaultValueSql("'trial'::platform.subscription_status_enum")
				.IsRequired();

			b.Property(x => x.StartsAt)
				.IsRequired();

			b.Property(x => x.AutoRenew)
				.HasDefaultValue(false)
				.IsRequired();

			b.ToTable(x => x.HasCheckConstraint(
				"CK_subscriptions_dates",
				"ends_at > starts_at"
				));

			b.HasIndex(x => new { x.TenantId, x.Status, x.EndsAt })
				.HasDatabaseName("subscriptions_current_idx")
				.IsDescending(false, false, true);

			b.HasIndex(x => new { x.TenantId, x.Id }).IsUnique();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.Subscriptions)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.SubscriptionPlan)
				.WithMany(x => x.Subscriptions)
				.HasForeignKey(x => x.PlanId)
				.OnDelete(DeleteBehavior.NoAction);
		}
	}
}
