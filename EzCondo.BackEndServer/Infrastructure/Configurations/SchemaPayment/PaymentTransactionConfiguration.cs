using EzCondo.BackEndServer.Data.Entities.SchemaPayment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaPayment
{
	internal sealed class PaymentTransactionConfiguration()
		: EntityConfigurationBase<PaymentTransaction>("payment_transactions", "payment")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<PaymentTransaction> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);

			b.Property(x => x.ProviderReference)
				.HasMaxLength(255);

			b.Property(x => x.ProviderTransactionId)
				.HasMaxLength(255)
				.IsRequired();

			b.Property(x => x.Amount)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.Property(x => x.OccurredAt)
				.HasColumnType("timestamptz")
				.IsRequired();

			b.Property(x => x.PayerMasked)
				.HasColumnType("jsonb")
				.HasDefaultValueSql("'{}'::jsonb")
				.IsRequired();

			b.Property(x => x.ProviderPayload)
				.HasColumnType("jsonb")
				.HasDefaultValueSql("'{}'::jsonb")
				.IsRequired();

			b.Property(x => x.Currency)
				.HasColumnType("char(3)")
				.HasDefaultValue("VND")
				.IsRequired();

			b.Property(x => x.Status)
				.HasColumnType("payment.payment_transaction_status")
				.IsRequired();

			b.HasOne(x => x.ProviderAccount)
				.WithMany(x => x.PaymentTransactions)
				.HasForeignKey(x => x.ProviderAccountId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.PaymentRequests)
				.WithMany(x => x.PaymentTransactions)
				.HasForeignKey(x => new { x.ProviderAccountId, x.PaymentRequestId })
				.HasPrincipalKey(x => new { x.ProviderAccountId, x.Id })
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.SourceWebhookEvent)
			.WithMany()
			.HasForeignKey(x => new
			{
				x.ProviderAccountId,
				x.SourceWebhookEventId
			})
			.HasPrincipalKey(x => new
			{
				x.ProviderAccountId,
				x.Id
			})
			.OnDelete(DeleteBehavior.Restrict);

			b.HasIndex(x => new { x.ProviderAccountId, x.Id })
				.IsUnique();

			b.HasIndex(x => new { x.ProviderAccountId, x.ProviderTransactionId })
				.IsUnique();

			b.HasIndex(x => new { x.ProviderAccountId, x.PaymentRequestId, x.OccurredAt })
				.HasDatabaseName("ix_payment_transactions_request");

			b.HasIndex(x => new { x.ProviderAccountId, x.OccurredAt })
				.HasDatabaseName("ix_payment_transactions_unresolved")
				.HasFilter("payment_request_id IS NULL");

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"ck_payment_transaction_amount",
					"(amount > 0)"
				);

				x.HasCheckConstraint(
					"ck_payment_transaction_verified",
					"status <> 'verified' OR payment_request_id IS NOT NULL"
				);
			});

		}
	}
}
