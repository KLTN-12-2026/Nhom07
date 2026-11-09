using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaPlatform
{
	internal sealed class SubscriptionPaymentAttemptConfiguration()
		: EntityConfigurationBase<SubscriptionPaymentAttempt>("subscription_payment_attempts", "platform")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<SubscriptionPaymentAttempt> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);

			b.Property(x => x.Status)
				.HasColumnType("platform.subscription_payment_attempt_status")
				.IsRequired();

			b.HasOne(x => x.SubscriptionInvoice)
				.WithMany(x => x.SubscriptionPaymentAttempts)
				.HasForeignKey(x => x.SubscriptionInvoiceId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.ProviderAccount)
				.WithMany(x => x.SubscriptionPaymentAttempts)
				.HasForeignKey(x => x.GatewayAccountId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.PaymentRequests)
				.WithMany(x => x.SubscriptionPaymentAttempts)
				.HasForeignKey(x => new { x.GatewayAccountId, x.GatewayPaymentRequestId })
				.HasPrincipalKey(x => new { x.ProviderAccountId, x.Id })
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.RequestedByUser)
				.WithMany(x => x.SubscriptionPaymentAttempts)
				.HasForeignKey(x => x.RequestedByUserId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasIndex(x => x.GatewayPaymentRequestId)
				.IsUnique();

			b.HasIndex(x => new { x.GatewayAccountId, x.Id })
				.IsUnique();

			b.HasIndex(x => new { x.SubscriptionInvoiceId, x.CreatedAt })
				.HasDatabaseName("ix_subscription_attempt_invoice")
				.IsDescending(false, true);

		}
	}
}
