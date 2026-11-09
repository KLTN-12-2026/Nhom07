using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaTenant
{
	internal sealed class PayrollDisbursmentConfiguration()
		: EntityConfigurationBase<PayrollDisbursement>("payroll_disbursements", "tenant")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<PayrollDisbursement> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);

			b.Property(x => x.ApprovedPayrollVersion)
				.HasColumnType("integer")
				.IsRequired();

			b.Property(x => x.ApprovedContentHash)
				.HasColumnType("bytea")
				.IsRequired();

			b.Property(x => x.ApprovedAt)
				.HasColumnType("timestamptz")
				.IsRequired();

			b.Property(x => x.IssuedAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.Status)
				.HasColumnType("tenant.payroll_disbursement_status")
				.IsRequired();

			b.Property(x => x.TotalAmount)
				.HasColumnType("numeric(18,2)")
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.PayrollDisbursements)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.Payroll)
				.WithMany(x => x.PayrollDisbursements)
				.HasForeignKey(x => new { x.TenantId, x.PayrollId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.ProviderAccount)
				.WithMany(x => x.PayrollDisbursements)
				.HasForeignKey(x => x.GatewayAccountId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.PayoutBatch)
				.WithMany(x => x.PayrollDisbursements)
				.HasForeignKey(x => new { x.GatewayAccountId, x.GatewayPayoutBatchId })
				.HasPrincipalKey(x => new { x.ProviderAccountId, x.Id })
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.ApprovedByUser)
				.WithMany(x => x.ApprovedBy)
				.HasForeignKey(x => x.ApprovedByUserId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.IssuedByUser)
				.WithMany(x => x.IssuedBy)
				.HasForeignKey(x => x.IssuedByUserId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasIndex(x => new { x.TenantId, x.Id })
				.IsUnique();

			b.HasIndex(x => new { x.TenantId, x.PayrollId, x.ApprovedPayrollVersion })
				.IsUnique();

			b.HasIndex(x => new { x.GatewayAccountId, x.GatewayPayoutBatchId })
				.HasDatabaseName("ux_payroll_disbursement_gateway_batch")
				.IsUnique()
				.HasFilter("gateway_payout_batch_id IS NOT NULL");

			b.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt })
				.HasDatabaseName("ix_payroll_disbursement_status");

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"ck_payroll_disbursement_version",
					"(approved_payroll_version > 0)");

				x.HasCheckConstraint(
					"ck_payroll_disbursement_hash",
					"(octet_length(approved_content_hash) = 32)");

				x.HasCheckConstraint(
					"ck_payroll_disbursement_amount",
					"(total_amount > 0)");

				x.HasCheckConstraint(
					"ck_payroll_disbursement_issued",
					"((issued_at IS NULL AND issued_by_user_id IS NULL) OR (issued_at IS NOT NULL AND issued_by_user_id IS NOT NULL))");

				x.HasCheckConstraint(
					"ck_payroll_disbursement_status_issued",
					"status = 'ready' OR issued_at IS NOT NULL");
			});
		}
	}
}
