using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class TenantPaymentTransactionConfiguration()
		: EntityConfigurationBase<TenantPaymentTransaction>("tenant_payment_transactions", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<TenantPaymentTransaction> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);

			b.Property(x => x.ProviderReference)
				.HasMaxLength(255)
				.IsRequired();

			b.Property(x => x.ProviderTransactionId)
				.HasMaxLength(255);

			b.Property(x => x.Status)
				.HasColumnType("tenant.payment_transaction_status")
				.HasDefaultValueSql("'received'::tenant.payment_transaction_status")
				.IsRequired();

			b.Property(x => x.Amount)
				.HasPrecision(18, 2)
				.IsRequired();

			b.Property(x => x.OccurredAt)
				.IsRequired();

			b.Property(x => x.PayerMasked)
				.HasColumnType("jsonb")
				.HasDefaultValueSql("'{}'::jsonb")
				.IsRequired();

			b.Property(x => x.ProviderPayload)
				.HasColumnType("jsonb")
				.HasDefaultValueSql("'{}'::jsonb")
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany()
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.PaymentOrdrer)
				.WithMany()
				.HasForeignKey(x => new { x.TenantId, x.PaymentOrderId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.ToTable(x => x.HasCheckConstraint(
				"CK_payment_transactions_amount",
				"amount > 0"
				));

			b.HasAlternateKey(x => new { x.TenantId, x.Id });
		}
	}
}
