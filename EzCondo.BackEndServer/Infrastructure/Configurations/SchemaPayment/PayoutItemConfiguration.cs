using EzCondo.BackEndServer.Data.Entities.SchemaPayment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaPayment
{
	internal sealed class PayoutItemConfiguration()
		: EntityConfigurationBase<PayoutItem>("payout_items", "payment")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<PayoutItem> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);

			b.Property(x => x.ReferenceId)
				.HasMaxLength(150)
				.IsRequired();

			b.Property(x => x.ProviderTransactionId)
				.HasMaxLength(255);

			b.Property(x => x.Amount)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.Property(x => x.Currency)
				.HasColumnType("char(3)")
				.HasDefaultValue("VND")
				.IsRequired();

			b.Property(x => x.Description)
				.HasMaxLength(255)
				.IsRequired();

			b.Property(x => x.DestinationBin)
				.HasMaxLength(12)
				.IsRequired();

			b.Property(x => x.DestinationAccountCiphertext)
				.HasColumnType("bytea")
				.IsRequired();

			b.Property(x => x.DestinationAccountLast4)
				.HasMaxLength(4)
				.IsRequired();

			b.Property(x => x.DestinationAccountName)
				.HasMaxLength(255)
				.IsRequired();

			b.Property(x => x.Status)
				.HasColumnType("payment.payout_item_status_enum")
				.IsRequired();

			b.Property(x => x.ProviderStatus)
				.HasMaxLength(80);

			b.Property(x => x.FailureCode)
				.HasMaxLength(80);

			b.Property(x => x.FailureMessage)
				.HasColumnType("text");

			b.HasOne(x => x.ProviderAccount)
				.WithMany(x => x.PayoutItems)
				.HasForeignKey(x => x.ProviderAccountId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.PayoutBatch)
				.WithMany(x => x.PayoutItems)
				.HasForeignKey(x => new
				{
					x.ProviderAccountId,
					x.PayoutBatchId
				})
				.HasPrincipalKey(x => new
				{
					x.ProviderAccountId,
					x.Id
				})
				.OnDelete(DeleteBehavior.Restrict);

			b.HasIndex(x => new { x.ProviderAccountId, x.Id })
				.IsUnique();

			b.HasIndex(x => new { x.ProviderAccountId, x.PayoutBatchId, x.ReferenceId })
				.IsUnique();

			b.HasIndex(x => new { x.ProviderAccountId, x.ProviderTransactionId })
				.HasDatabaseName("ux_payout_items_provider_transaction")
				.IsUnique()
				.HasFilter("provider_transaction_id IS NOT NULL");

			b.HasIndex(x => new { x.ProviderAccountId, x.PayoutBatchId, x.Status })
				.HasDatabaseName("ix_payout_items_batch_status");

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"ck_payout_items_amount",
					"(amount > 0)");

				x.HasCheckConstraint(
					"ck_payout_items_currency",
					"(currency = 'VND')");

				x.HasCheckConstraint(
					"ck_payout_items_last4",
					"(destination_account_last4 ~ '^[0-9]{4}$')");

				x.HasCheckConstraint(
					"ck_payout_items_failure",
					"status <> 'failed' OR failure_message IS NOT NULL OR failure_code IS NOT NULL");
			});
		}
	}
}
