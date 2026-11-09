using EzCondo.BackEndServer.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace EzCondo.BackEndServer.Contracts.Tenants
{
	public sealed record TenantResponse
	(
		Guid Id,
		string LegalName,
		string TaxCode,
		string Address,
		string Code,
		string Slug,
		DateTimeOffset? TrialEndsAt,
		TenantStatusEnum Status,
		TenantSubscriptionResponse Subscription,
		DateTimeOffset CreatedAt,
		DateTimeOffset? UpdatedAt
	);
}
