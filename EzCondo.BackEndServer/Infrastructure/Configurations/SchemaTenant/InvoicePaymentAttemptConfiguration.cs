using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaTenant
{
	internal sealed class InvoicePaymentAttemptConfiguration()
		: EntityConfigurationBase<InvoicePaymentAttempt>("invoice_payment_attempts", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<InvoicePaymentAttempt> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);

			b.Property(x => x.Status)
				.HasColumnType("tenant.invoice_payment_attempt_status")
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.InvoicePaymentAttempts)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.Invoice)
				.WithMany(x => x.InvoicePaymentAttempts)
				.HasForeignKey(x => new { x.TenantId, x.InvoiceId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.ProviderAccount)
				.WithMany(x => x.InvoicePaymentAttempts)
				.HasForeignKey(x => x.GatewayAccountId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.PaymentRequest)
				.WithMany(x => x.InvoicePaymentAttempts)
				.HasForeignKey(x => new { x.GatewayAccountId, x.GatewayPaymentRequestId })
				.HasPrincipalKey(x => new { x.ProviderAccountId, x.Id })
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.User)
				.WithMany(x => x.InvoicePaymentAttempts)
				.HasForeignKey(x => x.RequestedByUserId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasIndex(x => new { x.TenantId, x.Id })
				.IsUnique();

			b.HasIndex(x => new { x.GatewayAccountId, x.Id })
				.IsUnique();

			b.HasIndex(x => x.GatewayPaymentRequestId)
				.IsUnique();

			b.HasIndex(x => new { x.TenantId, x.InvoiceId, x.CreatedAt })
				.HasDatabaseName("ix_invoice_payment_attempt_invoice")
				.IsDescending(false, false, true);

			b.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt })
				.HasDatabaseName("ix_invoice_payment_attempt_open")
				.HasFilter("status IN ('pending', 'partially_paid')");

			b.HasAlternateKey(x => new
			{
				x.TenantId,
				x.GatewayAccountId,
				x.Id
			});
		}
	}
}
