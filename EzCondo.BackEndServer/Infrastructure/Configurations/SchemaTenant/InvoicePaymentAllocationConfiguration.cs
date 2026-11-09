using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaTenant
{
	internal sealed class InvoicePaymentAllocationConfiguration()
		: EntityConfigurationBase<InvoicePaymentAllocation>("invoice_payment_allocations", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<InvoicePaymentAllocation> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);

			b.Property(x => x.AmountApplied)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.InvoicePaymentAllocations)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.InvoicePaymentAttempt)
			.WithMany(x => x.InvoicePaymentAllocations)
			.HasForeignKey(x => new
			{
				x.TenantId,
				x.GatewayAccountId,
				x.PaymentAttemptId
			})
			.HasPrincipalKey(x => new
			{
				x.TenantId,
				x.GatewayAccountId,
				x.Id
			})
			.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.PaymentTransaction)
				.WithMany(x => x.InvoicePaymentAllocations)
				.HasForeignKey(x => new
				{
					x.GatewayAccountId,
					x.GatewayTransactionId
				})
				.HasPrincipalKey(x => new
				{
					x.ProviderAccountId,
					x.Id
				})
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.ProviderAccount)
				.WithMany(x => x.InvoicePaymentAllocations)
				.HasForeignKey(x => x.GatewayAccountId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasIndex(x => new { x.GatewayAccountId, x.GatewayTransactionId })
				.IsUnique();

			b.HasIndex(x => new { x.TenantId, x.PaymentAttemptId })
				.HasDatabaseName("ix_invoice_allocation_attempt");

			b.ToTable(x => x.HasCheckConstraint(
				"ck_invoice_allocation_amount",
				"(amount_applied > 0)"));

		}
	}
}
