using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class SubscriptionPlanConfiguration()
		: EntityConfigurationBase<SubscriptionPlan>("subscription_plans", "platform")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<SubscriptionPlan> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);
			ConfigureIsDeleted(b);
			ConfigureDeletedAt(b);

			b.Property(x => x.Name)
				.HasMaxLength(50)
				.IsRequired();

			b.Property(x => x.Description)
				.HasColumnType("text");

			b.Property(x => x.BillingCycleMonths)
				.HasColumnType("smallint")
				.HasDefaultValue(6)
				.IsRequired();

			b.Property(x => x.Price)
				.HasPrecision(18, 2)
				.IsRequired();

			b.Property(x => x.FeatureLimits)
				.HasColumnType("jsonb")
				.HasDefaultValueSql("'{}'::jsonb")
				.IsRequired();

			b.Property(x => x.IsAvailable)
				.HasDefaultValue(true)
				.IsRequired();

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"CK_subscription_plans_billing_cycle_months",
					"billing_cycle_months IN (6, 12, 24)"
					);

				x.HasCheckConstraint(
					"CK_subscription_plans_price",
					"price >= 0"
					);
			});
		}
	}
}
