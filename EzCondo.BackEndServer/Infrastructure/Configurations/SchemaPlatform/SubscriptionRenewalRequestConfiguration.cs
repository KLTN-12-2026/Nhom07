using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaPlatform
{
	internal sealed class SubscriptionRenewalRequestConfiguration()
		: EntityConfigurationBase<SubscriptionRenewalRequest>(
			"subscription_renewal_requests",
			"platform")
	{
		protected override void ConfigureEntity(
			EntityTypeBuilder<SubscriptionRenewalRequest> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);

			b.Property(x => x.CurrentEndsAtSnapshot)
				.HasColumnType("date")
				.IsRequired();

			b.Property(x => x.Status)
				.HasColumnType(
					"platform.subscription_renewal_request_status")
				.IsRequired();

			b.Property(x => x.PlanNameSnapshot)
				.HasMaxLength(50);

			b.Property(x => x.ExpiryNoticeLastAttemptAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.ExpiryNoticeSentAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.ExpiryNoticeLastError)
				.HasMaxLength(1000);

			b.Property(x => x.BillingCycleMonthsSnapshot)
				.HasColumnType("smallint");

			b.Property(x => x.PriceSnapshot)
				.HasColumnType("numeric(18,2)");

			b.Property(x => x.CurrencySnapshot)
				.HasColumnType("char(3)");

			b.Property(x => x.BillingPeriodFrom)
				.HasColumnType("date");

			b.Property(x => x.BillingPeriodTo)
				.HasColumnType("date");

			b.Property(x => x.TenantRespondedAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.PlatformReviewedAt)
				.HasColumnType("timestamptz");

			b.Property(x => x.ActivatedAt)
				.HasColumnType("timestamptz");

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.SubscriptionRenewalRequests)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.Subscription)
				.WithMany(x => x.RenewalRequests)
				.HasForeignKey(x => new
				{
					x.TenantId,
					x.SubscriptionId
				})
				.HasPrincipalKey(x => new
				{
					x.TenantId,
					x.Id
				})
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.SelectedPlan)
				.WithMany()
				.HasForeignKey(x => x.SelectedPlanId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.TenantRespondedByUser)
				.WithMany()
				.HasForeignKey(x => x.TenantRespondedByUserId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.PlatformReviewedByAdmin)
				.WithMany()
				.HasForeignKey(x => x.PlatformReviewedByAdminId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasOne(x => x.SubscriptionInvoice)
				.WithOne()
				.HasForeignKey<SubscriptionRenewalRequest>(
					x => x.SubscriptionInvoiceId)
				.OnDelete(DeleteBehavior.Restrict);

			b.HasIndex(x => new
			{
				x.TenantId,
				x.SubscriptionId,
				x.CurrentEndsAtSnapshot
			})
				.IsUnique()
				.HasDatabaseName(
					"ux_subscription_renewal_requests_period");

			b.HasIndex(x => new
			{
				x.TenantId,
				x.Status,
				x.CurrentEndsAtSnapshot
			})
				.HasDatabaseName(
					"ix_subscription_renewal_requests_tenant_status");

			b.HasIndex(x => new
			{
				x.Status,
				x.BillingPeriodFrom
			})
				.HasDatabaseName(
					"ix_subscription_renewal_requests_activation");

			b.HasIndex(x => x.SubscriptionInvoiceId)
				.IsUnique()
				.HasFilter("subscription_invoice_id IS NOT NULL")
				.HasDatabaseName(
					"ux_subscription_renewal_requests_invoice");

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"ck_subscription_renewal_requests_selection",
					"""
					(
						selected_plan_id IS NULL
						AND plan_name_snapshot IS NULL
						AND billing_cycle_months_snapshot IS NULL
						AND price_snapshot IS NULL
						AND currency_snapshot IS NULL
						AND billing_period_from IS NULL
						AND billing_period_to IS NULL
					)
					OR
					(
						selected_plan_id IS NOT NULL
						AND plan_name_snapshot IS NOT NULL
						AND billing_cycle_months_snapshot IN (6, 12, 24)
						AND price_snapshot > 0
						AND currency_snapshot IS NOT NULL
						AND billing_period_from IS NOT NULL
						AND billing_period_to IS NOT NULL
						AND billing_period_from = current_ends_at_snapshot
						AND billing_period_to = (
							billing_period_from
							+ make_interval(
								months => billing_cycle_months_snapshot
							)
						)::date
					)
					""");

				x.HasCheckConstraint(
					"ck_subscription_renewal_requests_status_payload",
					"""
					(
						status = 'awaiting_tenant_decision'
						AND tenant_responded_by_user_id IS NULL
						AND selected_plan_id IS NULL
						AND platform_reviewed_by_admin_id IS NULL
						AND subscription_invoice_id IS NULL
					)
					OR
					(
						status = 'tenant_declined'
						AND tenant_responded_by_user_id IS NOT NULL
						AND selected_plan_id IS NULL
						AND platform_reviewed_by_admin_id IS NULL
						AND subscription_invoice_id IS NULL
					)
					OR
					(
						status = 'awaiting_platform_review'
						AND tenant_responded_by_user_id IS NOT NULL
						AND selected_plan_id IS NOT NULL
						AND platform_reviewed_by_admin_id IS NULL
						AND subscription_invoice_id IS NULL
					)
					OR
					(
						status IN (
							'invoice_draft_created',
							'invoice_issued',
							'paid_awaiting_activation',
							'activated'
						)
						AND tenant_responded_by_user_id IS NOT NULL
						AND selected_plan_id IS NOT NULL
						AND platform_reviewed_by_admin_id IS NOT NULL
						AND subscription_invoice_id IS NOT NULL
					)
					OR status = 'expired'
					""");

				x.HasCheckConstraint(
					"ck_subscription_renewal_requests_tenant_response",
					"""
					(
						tenant_responded_by_user_id IS NULL
						AND tenant_responded_at IS NULL
					)
					OR
					(
						tenant_responded_by_user_id IS NOT NULL
						AND tenant_responded_at IS NOT NULL
					)
					""");

				x.HasCheckConstraint(
					"ck_subscription_renewal_requests_platform_review",
					"""
					(
						platform_reviewed_by_admin_id IS NULL
						AND platform_reviewed_at IS NULL
					)
					OR
					(
						platform_reviewed_by_admin_id IS NOT NULL
						AND platform_reviewed_at IS NOT NULL
					)
					""");

				x.HasCheckConstraint(
					"ck_subscription_renewal_requests_activation",
					"""
					(
						status = 'activated'
						AND activated_at IS NOT NULL
					)
					OR
					(
						status <> 'activated'
						AND activated_at IS NULL
					)
					""");

				x.HasCheckConstraint(
					"ck_subscription_renewal_requests_invoice",
					"""
					status = 'expired'
					OR
					(
						status IN (
							'awaiting_tenant_decision',
							'tenant_declined',
							'awaiting_platform_review'
						)
						AND subscription_invoice_id IS NULL
					)
					OR
					(
						status IN (
							'invoice_draft_created',
							'invoice_issued',
							'paid_awaiting_activation',
							'activated'
						)
						AND subscription_invoice_id IS NOT NULL
					)
					""");
			});
		}
	}
}
