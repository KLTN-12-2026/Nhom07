using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class TenantWebhookEventConfiguration()
		: EntityConfigurationBase<TenantPaymentWebhookEvent>("tenant_webhook_events", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<TenantPaymentWebhookEvent> b)
		{
			ConfigureUuidPrimaryKey(b);

			b.Property(x => x.EventKey)
				.HasMaxLength(255);

			b.Property(x => x.IsSignatureValid)
				.HasDefaultValue(false)
				.IsRequired();

			b.Property(x => x.Payload)
				.HasColumnType("jsonb")
				.IsRequired();

			b.Property(x => x.ReceivedAt)
				.HasColumnType("timestamptz")
				.HasDefaultValueSql("now()")
				.IsRequired();

			b.Property(x => x.ProcessedAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.ProcessingError)
				.HasColumnType("text");

			b.HasOne(x => x.Tenant)
				.WithMany()
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.PaymentProviderConfig)
				.WithMany()
				.HasForeignKey(x => new {x.TenantId, x.ProviderConfigId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);
		}
	}
}
