using EzCondo.BackEndServer.Data.Entities.SchemaPayment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaPayment
{
	internal sealed class PayoutBatchConfiguration()
		: EntityConfigurationBase<PayoutBatch>("payout_batches", "payment")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<PayoutBatch> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);

			b.Property(x => x.ReferenceId)
				.HasMaxLength(150)
				.IsRequired();

			b.Property(x => x.IdempotencyKey)
				.HasMaxLength(150)
				.IsRequired();

			b.Property(x => x.ProviderPayoutId)
				.HasMaxLength(255);

			b.Property(x => x.Status)
				.HasColumnType("payment.payout_batch_status")
				.IsRequired();

			b.Property(x => x.ProviderStatus)
				.HasMaxLength(80);

			b.Property(x => x.TotalAmount)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.Property(x => x.EstimatedFee)
				.HasColumnType("numeric(18,2)");

			b.Property(x => x.Currency)
				.HasColumnType("char(3)")
				.HasDefaultValue("VND")
				.IsRequired();

			b.Property(x => x.LastSyncedAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.ProviderPayload)
				.HasColumnType("jsonb")
				.HasDefaultValueSql("'{}'::jsonb")
				.IsRequired();

			b.HasOne(x => x.ProviderAccount)
				.WithMany(x => x.PayoutBatchs)
				.HasForeignKey(x => x.ProviderAccountId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasIndex(x => new { x.ProviderAccountId, x.Id })
				.IsUnique();

			b.HasIndex(x => new { x.ProviderAccountId, x.ReferenceId })
				.IsUnique();

			b.HasIndex(x => new { x.ProviderAccountId, x.IdempotencyKey })
				.IsUnique();

			b.HasIndex(x => new { x.ProviderAccountId, x.ProviderPayoutId })
				.HasDatabaseName("ux_payout_batches_provider_id")
				.IsUnique()
				.HasFilter("provider_payout_id IS NOT NULL");

			b.HasIndex(x => new { x.ProviderAccountId, x.Status, x.LastSyncedAt })
				.HasDatabaseName("ix_payout_batches_reconciliation")
				.HasFilter("status IN ('submitting', 'processing', 'partially_succeeded')");
		}
	}
}
