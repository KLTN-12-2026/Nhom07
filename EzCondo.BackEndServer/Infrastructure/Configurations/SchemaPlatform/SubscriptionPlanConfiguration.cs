using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaPlatform
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
					"CK_subscription_plans_price",
					"price >= 0"
					);
			});

			b.HasOne(x => x.AdminCreated)
				.WithMany(x => x.Created)
				.HasForeignKey(x => x.CreatedByAdmin)
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.AdminUpdated)
				.WithMany(x => x.Updated)
				.HasForeignKey(x => x.UpdatedByAdmin)
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.AdminDeleted)
				.WithMany(x => x.Deleted)
				.HasForeignKey(x => x.DeletedByAdmin)
				.OnDelete(DeleteBehavior.NoAction);
		}
	}
}
