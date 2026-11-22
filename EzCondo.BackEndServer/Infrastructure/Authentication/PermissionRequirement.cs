using Microsoft.AspNetCore.Authorization;

namespace EzCondo.BackEndServer.Infrastructure.Authentication
{
	public sealed record PermissionRequirement
		: IAuthorizationRequirement
	{
		public string PermissionCode { get; }

		public PermissionRequirement(
			string permissionCode)
		{
			if (string.IsNullOrWhiteSpace(permissionCode))
			{
				throw new ArgumentException(
					"Permission code không được để trống.",
					nameof(permissionCode));
			}

			PermissionCode = permissionCode.Trim();
		}
	}
}
