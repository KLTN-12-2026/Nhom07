using EzCondo.BackEndServer.Contracts.Authentication.Platform;
using EzCondo.BackEndServer.Contracts.Authentication.Tenant;
using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Infrastructure.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EzCondo.BackEndServer.Services.Authentication
{
	public sealed class AuthService(
		UserManager<User> userManager,
		SignInManager<User> signInManager,
		ApplicationDbContext dbcontext,
		IJwtTokenService jwtTokenService,
		TimeProvider timeProvider)
		: IAuthService
	{
		public async Task<PlatformLoginResult> LoginPlatformAsync(
			PlatformLoginRequest request,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(request);

			var email = request.Email.Trim();

			var user = await userManager.FindByEmailAsync(email);

			if(user is null)
			{
				return PlatformLoginResult.Failed(LoginFailure.InvalidCredentials);
			}

			if(user.TenantId is not null || !CanSignIn(user)){
				return PlatformLoginResult.Failed(LoginFailure.AccountUnavailable);
			}

			var passwordResult =
				await signInManager.CheckPasswordSignInAsync(
					user,
					request.Password,
					lockoutOnFailure: true);

			if (passwordResult.IsLockedOut)
			{
				return PlatformLoginResult.Failed(LoginFailure.LockedOut);
			}

			if (!passwordResult.Succeeded)
			{
				return PlatformLoginResult.Failed(LoginFailure.InvalidCredentials);
			}

			var roleCodes = await dbcontext.UserRoles
				.AsNoTracking()
				.Where(x =>
					x.UserId == user.Id &&
					x.Scope == RoleScopeEnum.Platform &&
					x.TenantId == null &&
					x.RevokedAt == null &&
					x.Role.Scope == RoleScopeEnum.Platform &&
					x.Role.TenantId == null &&
					x.Role.Code == DbInitializer.PlatformAdminRoleCode &&
					!x.Role.IsDeleted)
				.Select(x => x.Role.Code)
				.Distinct()
				.OrderBy(x => x)
				.ToListAsync(cancellationToken);

			if(roleCodes.Count != 1)
			{
				return PlatformLoginResult.Failed(LoginFailure.NoActiveRole);
			}

			var accessToken = jwtTokenService.CreateAccessToken(
				user,
				RoleScopeEnum.Platform,
				roleCodes);

			return PlatformLoginResult.Success(
				new PlatformLoginResponse(
					accessToken.Token,
					"Bearer",
					accessToken.ExpiresAt,
					user.Id,
					user.Email!,
					user.FullName,
					roleCodes));
		}

		public async Task<TenantLoginResult> LoginTenantAsync(
			TenantLoginRequest request,
			CancellationToken cancellationToken = default,
			Guid? mobileSessionId = null)
		{
			ArgumentNullException.ThrowIfNull(request);

			var tenantSlug = request.TenantSlug.Trim();
			var email = request.Email.Trim();
			var now = timeProvider.GetUtcNow();

			var tenant = await dbcontext.Tenants
				.AsNoTracking()
				.SingleOrDefaultAsync(
					x => x.Slug == tenantSlug,
					cancellationToken);

			if(tenant is null)
			{
				return TenantLoginResult.Failed(LoginFailure.TenantUnavailable);
			}

			var tenantIsAvailable =
				tenant.Status == TenantStatusEnum.Active ||
				(
					tenant.Status == TenantStatusEnum.Trial &&
					tenant.TrialEndsAt.HasValue &&
					tenant.TrialEndsAt.Value > now
				);

			if (!tenantIsAvailable)
			{
				return TenantLoginResult.Failed(LoginFailure.TenantUnavailable);
			}

			var user = await userManager.FindByEmailAsync(email);

			if(user is null)
			{
				return TenantLoginResult.Failed(LoginFailure.InvalidCredentials);
			}

			if(user.TenantId != tenant.Id || !CanSignIn(user))
			{
				return TenantLoginResult.Failed(LoginFailure.AccountUnavailable);
			}

			var passwordResult = await signInManager.CheckPasswordSignInAsync(
				user,
				request.Password,
				lockoutOnFailure: true);

			if (passwordResult.IsLockedOut)
			{
				return TenantLoginResult.Failed(LoginFailure.LockedOut);
			}

			if (!passwordResult.Succeeded)
			{
				return TenantLoginResult.Failed(LoginFailure.InvalidCredentials);
			}

			var roleCodes = await dbcontext.UserRoles
				.AsNoTracking()
				.Where(x =>
					x.UserId == user.Id &&
					x.Scope == RoleScopeEnum.Tenant &&
					x.TenantId == tenant.Id &&
					x.RevokedAt == null &&
					x.Role.Scope == RoleScopeEnum.Tenant &&
					x.Role.TenantId == tenant.Id &&
					!x.Role.IsDeleted)
				.Select(x => x.Role.Code)
				.Distinct()
				.OrderBy(x => x)
				.ToListAsync(cancellationToken);

			if(roleCodes.Count == 0)
			{
				return TenantLoginResult.Failed(LoginFailure.NoActiveRole);
			}

			var accessToken = jwtTokenService.CreateAccessToken(
				user,
				RoleScopeEnum.Tenant,
				roleCodes,
				mobileSessionId);

			return TenantLoginResult.Success(
				new TenantLoginResponse(
					accessToken.Token,
					"Bearer",
					accessToken.ExpiresAt,
					user.Id,
					user.Email!,
					user.FullName,
					tenant.Id,
					tenant.Slug,
					roleCodes));
		}

		public async Task<LogoutResult> LogoutAllAsync(
			Guid userId,
			CancellationToken cancellationToken = default)
		{
			var user = await userManager.FindByIdAsync(userId.ToString());

			if (user is null || user.IsDeleted)
			{
				return LogoutResult.Failed(LogoutFailure.UserNotFound);
			}

			var updateUser = await userManager.UpdateSecurityStampAsync(user);

			if (!updateUser.Succeeded)
			{
				return LogoutResult.Failed(LogoutFailure.UpdateSecurityStampFailed);
			}

			return LogoutResult.Success();
		}

		private static bool CanSignIn(User user)
		{
			return !user.IsDeleted &&
				user.Status == UserStatusEnum.Active &&
				user.EmailConfirmed;
		}
	}
}
