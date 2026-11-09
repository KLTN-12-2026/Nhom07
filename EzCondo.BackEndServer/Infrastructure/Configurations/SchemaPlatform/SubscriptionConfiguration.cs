using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaPlatform
{
	internal sealed class SubscriptionConfiguration()
		: EntityConfigurationBase<Subscription>("subscriptions", "platform")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<Subscription> b)
		{
			ConfigureUuidPrimaryKey(b);

			b.Property(x => x.BillingCycleMonthsSnapshot)
				.HasColumnType("smallint")
				.IsRequired();

			b.Property(x => x.Status)
				.HasColumnType("platform.subscription_status_enum")
				.IsRequired();

			b.Property(x => x.StartsAt)
				.HasColumnType("date")
				.IsRequired();

			b.Property(x => x.EndsAt)
				.HasColumnType("date")
				.IsRequired();

			b.Property(x => x.CancelledAt)
				.HasColumnType("timestamptz");

			b.ToTable(t =>
			{
				t.HasCheckConstraint(
					"ck_subscriptions_billing_cycle",
					"billing_cycle_months_snapshot > 0");
				t.HasCheckConstraint(
					"ck_subscriptions_period",
					"ends_at > starts_at");
				t.HasCheckConstraint(
					"ck_subscriptions_cancelled",
					"status <> 'cancelled' OR cancelled_at IS NOT NULL");
			});

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.Subscriptions)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.Plan)
				.WithMany(x => x.Subscriptions)
				.HasForeignKey(x =>  x.PlanId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasAlternateKey(x => new { x.TenantId, x.Id });

			b.HasIndex(x => x.TenantId)
				.HasDatabaseName("ux_subscriptions_tenant_active")
				.IsUnique()
				.HasFilter("status IN ('trial', 'active', 'past_due')");
		}
	}
}
