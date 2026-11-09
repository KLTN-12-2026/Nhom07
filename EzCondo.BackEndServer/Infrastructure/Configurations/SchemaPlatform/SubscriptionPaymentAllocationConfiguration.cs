using EzCondo.BackEndServer.Data.Entities.SchemaPayment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaPlatform
{
	internal sealed class SubscriptionPaymentAllocationConfiguration()
		: EntityConfigurationBase<SubscriptionPaymentAllocation>("subscription_payment_allocations", "platform")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<SubscriptionPaymentAllocation> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);

			b.Property(x => x.AmountApplied)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.HasOne(x => x.SubscriptionPaymentAttempt)
				.WithMany(x => x.SubscriptionPaymentAllocations)
				.HasForeignKey(x => new { x.GatewayAccountId, x.PaymentAttemptId })
				.HasPrincipalKey(x => new { x.GatewayAccountId, x.Id })
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.PaymentTransaction)
				.WithMany(x => x.SubscriptionPaymentAllocations)
				.HasForeignKey(x => new { x.GatewayAccountId, x.GatewayTransactionId })
				.HasPrincipalKey(x => new { x.ProviderAccountId, x.Id })
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.ProviderAccount)
				.WithMany(x => x.SubscriptionPaymentAllocations)
				.HasForeignKey(x => x.GatewayAccountId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasIndex(x => new { x.GatewayAccountId, x.GatewayTransactionId })
				.IsUnique();

			b.ToTable(x => x.HasCheckConstraint(
				"ck_subscription_allocation_amount",
				"(amount_applied > 0)"));
		}
	}
}
