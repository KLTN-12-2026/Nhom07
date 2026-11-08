using EzCondo.BackEndServer.Data.Entities.SchemaPayment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaPayment
{
	internal sealed class PaymentWebhookEventConfiguration()
		: EntityConfigurationBase<PaymentWebhookEvent>("webhook_events", "payment")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<PaymentWebhookEvent> b)
		{
			ConfigureUuidPrimaryKey(b);

			b.Property(x => x.EventKey)
				.HasMaxLength(255);

			b.Property(x => x.OrderCode)
				.HasColumnType("bigint");

			b.Property(x => x.ProviderPaymentLinkId)
				.HasMaxLength(255);

			b.Property(x => x.ProviderReference)
				.HasMaxLength(255);

			b.Property(x => x.SignatureValid)
				.HasDefaultValue(false);

			b.Property(x => x.RawPayload)
				.HasColumnType("text");

			b.Property(x => x.ProcessingStatus)
				.HasColumnType("payment.payment_status_enum")
				.HasDefaultValueSql("'received'::payment.payment_status_enum")
				.IsRequired();

			b.Property(x => x.processingAttempts)
				.HasColumnType("integer")
				.IsRequired();

			b.Property(x => x.LastAttemptAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.NextAttemptAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.ProcessedAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.ReceivedAt)
				.HasColumnType("timestamptz")
				.HasDefaultValueSql("now()")
				.IsRequired();

			b.Property(x => x.ProcessedAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.ProcessingError)
				.HasColumnType("text");

			b.HasOne(x => x.ProviderAccount)
				.WithMany(x => x.PaymentWebhookEvents)
				.HasForeignKey(x => x.ProviderAccountId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.PaymentRequest)
				.WithMany(x => x.PaymentWebhookEvents)
				.HasForeignKey(x => new { x.ProviderAccountId, x.PaymentRequestId })
				.HasPrincipalKey(x => new { x.ProviderAccountId, x.Id })
				.OnDelete(DeleteBehavior.Restrict);

			b.HasIndex(x => new { x.ProviderAccountId, x.Id })
				.IsUnique();

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"ck_webhook_event_attempt",
					"processing_attempts > 0");

				x.HasCheckConstraint(
					"ck_webhook_event_event_key",
					"CHECK (event_key IS NULL OR (signature_valid = true AND btrim(event_key) <> ''))");

				x.HasCheckConstraint(
					"ck_webhook_event_resolved_request",
					"CHECK (payment_request_id IS NULL OR signature_valid = true)");

				x.HasCheckConstraint(
					"ck_webhook_event_processed",
					"CHECK (processed_at IS NULL OR (signature_valid = true AND processing_status = 'succeeded' AND payment_request_id IS NOT NULL AND processing_error IS NULL))");
			});

			b.HasIndex(x => new { x.ProviderAccountId, x.EventKey })
				.IsUnique();

			b.HasIndex(x => new { x.ProviderAccountId, x.OrderCode })
				.HasDatabaseName("ix_webhook_events_order_code");

			b.HasIndex(x => new { x.ProviderAccountId, x.ProviderPaymentLinkId })
				.HasDatabaseName("ix_webhook_events_provider_link")
				.HasFilter("provider_payment_link_id I NOT NULL");

			b.HasIndex(x => new { x.NextAttemptAt, x.ReceivedAt })
				.HasDatabaseName("ix_webhook_events_pending")
				.HasFilter("signature_valid = true AND processing_status IN ('received', 'failed') AND processing_at IS NULL");
		}
	}
}
