using System.Security.Claims;
using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EzCondo.BackEndServer.Infrastructure.Authentication
{
	public sealed class PermissionAuthorizationHandler(
		ApplicationDbContext dbContext)
		: AuthorizationHandler<PermissionRequirement>
	{
		protected override async Task HandleRequirementAsync(
			AuthorizationHandlerContext context,
			PermissionRequirement requirement)
		{
			var subject =
				context.User.FindFirstValue(
					AppClaimTypes.Subject);

			var scope =
				context.User.FindFirstValue(
					AppClaimTypes.Scope);

			if (!Guid.TryParse(subject, out var userId))
			{
				return;
			}

			if (!string.Equals(
				scope,
				"platform",
				StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			var userIsActive =
				await dbContext.Users
					.AsNoTracking()
					.AnyAsync(x =>
						x.Id == userId &&
						!x.IsDeleted &&
						x.Status == UserStatusEnum.Active);

			if (!userIsActive)
			{
				return;
			}

			var hasPermission =
				await (
					from userRole in dbContext.UserRoles
					join role in dbContext.Roles
						on userRole.RoleId equals role.Id
					join rolePermission in dbContext.RolePermissions
						on role.Id equals rolePermission.RoleId
					where
						userRole.UserId == userId &&
						userRole.Scope == RoleScopeEnum.Platform &&
						userRole.TenantId == null &&
						userRole.RevokedAt == null &&
						role.Scope == RoleScopeEnum.Platform &&
						role.TenantId == null &&
						!role.IsDeleted &&
						rolePermission.Scope ==
							RoleScopeEnum.Platform &&
						rolePermission.PermissionCode ==
							requirement.PermissionCode
					select userRole.Id)
				.AnyAsync();

			if (hasPermission)
			{
				context.Succeed(requirement);
			}
		}
	}
}