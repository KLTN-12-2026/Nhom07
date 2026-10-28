using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class SubscriptionEventConfiguration()
		: EntityConfigurationBase<SubscriptionEvent>("subscription_events", "platform")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<SubscriptionEvent> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);

			b.Property(x => x.EventType)
				.HasMaxLength(50)
				.IsRequired();

			b.Property(x => x.Data)
				.HasColumnType("jsonb")
				.HasDefaultValueSql("'{}'::jsonb")
				.IsRequired();

			b.Property(x => x.SubscriptionId)
				.IsRequired();

			b.HasOne(x => x.Subscription)
				.WithMany()
				.HasForeignKey(x => x.SubscriptionId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
