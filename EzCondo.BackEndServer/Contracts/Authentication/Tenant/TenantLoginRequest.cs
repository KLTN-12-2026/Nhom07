using System.ComponentModel.DataAnnotations;

namespace EzCondo.BackEndServer.Contracts.Authentication.Tenant
{
	public sealed record TenantLoginRequest
	(
		[Required]
		string TenantSlug,

		[Required, EmailAddress]
		string Email,

		[Required]
		string Password
	);
}
