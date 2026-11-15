using System.ComponentModel.DataAnnotations;

namespace EzCondo.BackEndServer.Contracts.Authentication.Platform
{
	public sealed record PlatformLoginRequest
	(
		[Required, EmailAddress]
		string Email,

		[Required]
		string Password
	);
}
