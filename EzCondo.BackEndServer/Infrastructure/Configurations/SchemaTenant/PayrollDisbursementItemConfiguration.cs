using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaTenant
{
	internal sealed class PayrollDisbursementItemConfiguration()
		: EntityConfigurationBase<PayrollDisbursementItem>("payroll_disbursement_items", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<PayrollDisbursementItem> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);

			b.Property(x => x.Amount)
				.HasColumnType("numeric(18,2)")
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

			b.Property(x => x.RecipientNameSnapshot)
				.HasMaxLength(255)
				.IsRequired();

			b.Property(x => x.Status)
				.HasColumnType("tenant.payroll_disbursement_item_status")
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.PayrollDisbursementItems)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.PayrollDisbursement)
				.WithMany(x => x.PayrollDisbursementItems)
				.HasForeignKey(x => new { x.TenantId, x.PayrollDisbursementId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.StaffUser)
				.WithMany(x => x.PayrollDisbursementItems)
				.HasForeignKey(x => x.StaffUserId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.PayoutItem)
				.WithMany(x => x.PayrollDisbursementItems)
				.HasForeignKey(x => new { x.GatewayAccountId, x.GatewayPayoutItemId })
				.HasPrincipalKey(x => new { x.ProviderAccountId, x.Id })
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.ProviderAccount)
				.WithMany(x => x.PayrollDisbursementItems)
				.HasForeignKey(x => x.GatewayAccountId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.PayrollDetail)
				.WithMany(x => x.PayrollDisbursementItems)
				.HasForeignKey(x => new { x.TenantId, x.PayrollDetailId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.Restrict);

			b.HasIndex(x => new { x.TenantId, x.PayrollDisbursementId, x.PayrollDetailId })
				.IsUnique();

			b.HasIndex(x => new { x.GatewayAccountId, x.GatewayPayoutItemId })
				.HasDatabaseName("ux_payroll_item_gateway")
				.IsUnique()
				.HasFilter("gateway_payout_item_id IS NOT NULL");

			b.HasIndex(x => new { x.TenantId, x.PayrollDisbursementId, x.Status })
				.HasDatabaseName("ix_payroll_item_status");

			

			b.ToTable(x => x.HasCheckConstraint(
				"ck_payroll_item_amount",
				"(amount > 0)"));
		}
	}
}
