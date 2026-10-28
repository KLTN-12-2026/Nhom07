using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class SubscriptionPaymentWebhookEventConfiguration()
		: EntityConfigurationBase<SubscriptionPaymentWebhookEvent>("subscription_payment_webhook_events", "platform")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<SubscriptionPaymentWebhookEvent> b)
		{
			ConfigureUuidPrimaryKey(b);
			b.Property(x => x.EventKey).HasMaxLength(255);
			b.Property(x => x.OrderCode).HasColumnType("bigint");
			b.Property(x => x.IsSignatureValid).HasDefaultValue(false).IsRequired();
			b.Property(x => x.Payload).HasColumnType("text").IsRequired();
			b.Property(x => x.ReceivedAt).HasColumnType("timestamptz")
				.HasDefaultValueSql("now()").IsRequired();
			b.Property(x => x.ProcessedAt).HasColumnType("timestamptz");
			b.Property(x => x.LastAttemptAt).HasColumnType("timestamptz");
			b.Property(x => x.ProcessingAttempts).HasDefaultValue(0).IsRequired();
			b.Property(x => x.ProcessingError).HasColumnType("text");

			b.HasOne(x => x.ProviderConfig).WithMany(x => x.WebhookEvents)
				.HasForeignKey(x => x.ProviderConfigId).OnDelete(DeleteBehavior.NoAction);
			b.HasOne(x => x.PaymentOrder).WithMany(x => x.WebhookEvents)
				.HasForeignKey(x => new { x.ProviderConfigId, x.PaymentOrderId })
				.HasPrincipalKey(x => new { x.ProviderConfigId, x.Id })
				.IsRequired(false).OnDelete(DeleteBehavior.NoAction);

			b.HasIndex(x => new { x.ProviderConfigId, x.EventKey })
				.IsUnique().HasFilter("event_key IS NOT NULL");
			b.HasIndex(x => new { x.ProviderConfigId, x.OrderCode });
			b.HasIndex(x => x.ReceivedAt)
				.HasFilter("is_signature_valid = TRUE AND processed_at IS NULL");
			b.ToTable(t =>
			{
				t.HasCheckConstraint("CK_subscription_webhook_attempts", "processing_attempts >= 0");
				t.HasCheckConstraint("CK_subscription_webhook_event_key",
					"event_key IS NULL OR (is_signature_valid = TRUE AND btrim(event_key) <> '')");
				t.HasCheckConstraint("CK_subscription_webhook_order_verified",
					"payment_order_id IS NULL OR is_signature_valid = TRUE");
				t.HasCheckConstraint("CK_subscription_webhook_processed",
					"processed_at IS NULL OR (is_signature_valid = TRUE AND payment_order_id IS NOT NULL AND processing_error IS NULL)");
			});
		}
	}
}
