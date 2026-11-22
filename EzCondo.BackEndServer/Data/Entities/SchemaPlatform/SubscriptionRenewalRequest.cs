using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaPlatform
{
	public sealed class SubscriptionRenewalRequest : IAuditableEntity
	{
		public Guid Id { get; set; }
		public Guid TenantId { get; set; }
		public Guid SubscriptionId { get; set; }
		public DateOnly CurrentEndsAtSnapshot { get; set; }
		public SubscriptionRenewalRequestStatus Status { get; set; }
			= SubscriptionRenewalRequestStatus.AwaitingTenantDecision;
		public Guid? SelectedPlanId { get; set; }
		public string? PlanNameSnapshot { get; set; }
		public int? BillingCycleMonthsSnapshot { get; set; }
		public decimal? PriceSnapshot { get; set; }
		public string? CurrencySnapshot { get; set; }
		public DateOnly? BillingPeriodFrom { get; set; }
		public DateOnly? BillingPeriodTo { get; set; }
		public DateTimeOffset? ExpiryNoticeLastAttemptAt { get; set; }
		public DateTimeOffset? ExpiryNoticeSentAt { get; set; }
		public string? ExpiryNoticeLastError { get; set; }
		public Guid? TenantRespondedByUserId { get; set; }
		public DateTimeOffset? TenantRespondedAt { get; set; }
		public Guid? PlatformReviewedByAdminId { get; set; }
		public DateTimeOffset? PlatformReviewedAt { get; set; }
		public Guid? SubscriptionInvoiceId { get; set; }
		public DateTimeOffset? ActivatedAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public Tenant Tenant { get; set; } = null!;
		public Subscription Subscription { get; set; } = null!;
		public SubscriptionPlan? SelectedPlan { get; set; }
		public User? TenantRespondedByUser { get; set; }
		public User? PlatformReviewedByAdmin { get; set; }
		public SubscriptionInvoice? SubscriptionInvoice { get; set; }
	}
}
