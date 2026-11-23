using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Contracts.PlatformDashboard
{
	public sealed record PlatformDashboardResponse(
		DashboardPeriodResponse Period,
		TenantStatisticsResponse Tenants,
		SubscriptionStatisticsResponse Subscriptions,
		RevenueStatisticsResponse Revenue,
		RenewalStatisticsResponse Renewals,
		UserStatisticsResponse Users,
		CommunicationStatisticsResponse Communications,
		ResourceUsageStatisticsResponse Resources,
		DashboardAvailabilityResponse Availability);

	public sealed record DashboardPeriodResponse(
		DateOnly From,
		DateOnly To,
		DateTimeOffset AsOf,
		string TimeZone,
		DashboardGranularity Granularity);

	public sealed record TenantStatisticsResponse(
		int TotalTenants,
		int OperationalTenants,
		IReadOnlyList<TenantStatusCountResponse> ByStatus);

	public sealed record TenantStatusCountResponse(
		TenantStatusEnum Status,
		int Count);

	public sealed record SubscriptionStatisticsResponse(
		int ActiveSubscriptions,
		int TrialSubscriptions,
		int PastDueSubscriptions,
		int ExpiringWithin7Days,
		int ExpiringWithin30Days,
		int ExpiringWithin60Days,
		IReadOnlyList<ExpiringSubscriptionResponse> ExpiringSubscriptions);

	public sealed record ExpiringSubscriptionResponse(
		Guid TenantId,
		string TenantCode,
		string TenantName,
		Guid SubscriptionId,
		Guid PlanId,
		string PlanName,
		SubscriptionStatusEnum SubscriptionStatus,
		TenantStatusEnum TenantStatus,
		DateOnly EndsAt);

	public sealed record RevenueStatisticsResponse(
		IReadOnlyList<MoneyResponse> TotalPaid,
		IReadOnlyList<RevenueSeriesItemResponse> Series,
		IReadOnlyList<RevenueByPlanResponse> ByPlan,
		int PaidInvoiceCount,
		int OutstandingInvoiceCount,
		IReadOnlyList<MoneyResponse> OutstandingAmount);

	public sealed record MoneyResponse(
		string Currency,
		decimal Amount);

	public sealed record RevenueSeriesItemResponse(
		string Period,
		string Currency,
		decimal Amount);

	public sealed record RevenueByPlanResponse(
		Guid PlanId,
		string PlanName,
		string Currency,
		decimal Amount);

	public sealed record RenewalStatisticsResponse(
		int EligibleCount,
		int ConfirmedCount,
		int DeclinedCount,
		int PendingCount,
		int ExpiredCount,
		decimal? RenewalRate);

	public sealed record UserStatisticsResponse(
		int ActiveUsers,
		int ActivePlatformUsers,
		int ActiveTenantUsers,
		int? MonthActiveUsers,
		bool ActivityTelemetryAvailable);

	public sealed record CommunicationStatisticsResponse(
		int InvitationEmailsSent,
		int RenewalEmailsSent,
		int KnownEmailsSent,
		int? OtpTelemetryAvailable);

	public sealed record ResourceUsageStatisticsResponse(
		bool Available,
		IReadOnlyList<ResourceUsageTenantResponse> Items);

	public sealed record ResourceUsageTenantResponse(
		Guid TenantId,
		string TenantCode,
		string TenantName,
		long? StorageBytes,
		long? DatabaseBytes,
		long? RequestCount);

	public sealed record DashboardAvailabilityResponse(
		bool FailedLoginTelemetryAvailable,
		bool OtpTelemetryAvailable,
		bool ResourceUsageAvailable,
		IReadOnlyList<string> Notes);
}
