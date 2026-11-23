using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Enums
{
	public enum SubscriptionRenewalRequestStatus
	{
		[PgName("awaiting_tenant_decision")]
		AwaitingTenantDecision,

		[PgName("tenant_declined")]
		TenantDeclined,

		[PgName("awaiting_platform_review")]
		AwaitingPlatformReview,

		[PgName("invoice_draft_created")]
		InvoiceDraftCreated,

		[PgName("invoice_issued")]
		InvoiceIssued,

		[PgName("paid_awaiting_activation")]
		PaidAwaitingActivation,

		[PgName("activated")]
		Activated,

		[PgName("expired")]
		Expired
	}
}
