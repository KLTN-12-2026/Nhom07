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

			b.Property(x => x.BillingCycleMonth)
				.HasColumnType("smallint")
				.IsRequired();

			b.Property(x => x.RegisteredAt)
				.HasColumnType("date")
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.Subscriptions)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.Plan)
				.WithMany(x => x.Subscriptions)
				.HasForeignKey(x =>  x.PlanId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
