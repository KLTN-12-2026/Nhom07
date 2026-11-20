using EzCondo.BackEndServer.Contracts.Invitations;
using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaSecurity;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Infrastructure.Invitations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EzCondo.BackEndServer.Services.Invitations
{
	public sealed class InvitationAcceptanceService(
		ApplicationDbContext dbcontext,
		UserManager<User> userManager,
		IInvitationTokenService invitationTokenService,
		TimeProvider timeProvider)
		: IInvitationAcceptanceService
	{
		public async Task<AcceptInvitationResult> AcceptAsync(
			AcceptInvitationRequest request, 
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(request);

			if(!string.Equals(
				request.Password,
				request.ConfirmPassword,
				StringComparison.Ordinal))
			{
				return AcceptInvitationResult.PasswordRejected(
					["Mật khẩu xác nhận không chính xác."]);
			}

			var rawToken = request.Token.Trim();

			string tokenHash;

			try
			{
				tokenHash = invitationTokenService.ComputeHash(rawToken);
			}
			catch (ArgumentException)
			{
				return AcceptInvitationResult.Failed(
					AcceptInvitationFailure.InvalidToken);
			}

			var now = timeProvider.GetUtcNow();

			await using var transaction =
				await dbcontext.Database.BeginTransactionAsync(
					cancellationToken);

			var invitation = await dbcontext.Invitations
				.FromSqlInterpolated(
					$"""
					SELECT *
					FROM tenant.invitations
					WHERE token_hash = {tokenHash}
					FOR UPDATE
					""")
				.SingleOrDefaultAsync(cancellationToken);

			if(invitation is null || invitation.IsDeleted)
			{
				return AcceptInvitationResult.Failed(
					AcceptInvitationFailure.InvalidToken);
			}

			if(invitation.Status == InvitationStatusEnum.Accepted ||
				invitation.AcceptedAt.HasValue)
			{
				return AcceptInvitationResult.Failed(
					AcceptInvitationFailure.InvitationAlreadyAccepted);
			}

			if(invitation.Status == InvitationStatusEnum.Revoked ||
				invitation.RevokedAt.HasValue)
			{
				return AcceptInvitationResult.Failed(
					AcceptInvitationFailure.InvitationRevoked);
			}
			
			if(invitation.Status == InvitationStatusEnum.Expired ||
				invitation.ExpiresAt <= now)
			{
				if(invitation.Status == InvitationStatusEnum.PendingDelivery ||
					invitation.Status == InvitationStatusEnum.Sent)
				{
					invitation.Status = InvitationStatusEnum.Expired;

					await dbcontext.SaveChangesAsync(cancellationToken);

					await transaction.CommitAsync(cancellationToken);
				}

				return AcceptInvitationResult.Failed(
					AcceptInvitationFailure.InvitationExpired);
			}

			if(invitation.Status == InvitationStatusEnum.PendingDelivery)
			{
				return AcceptInvitationResult.Failed(
					AcceptInvitationFailure.InvitationNotSent);
			}

			if(invitation.Status != InvitationStatusEnum.Sent)
			{
				return AcceptInvitationResult.Failed(
					AcceptInvitationFailure.InvalidToken);
			}

			var tenant = await dbcontext.Tenants
				.FromSqlInterpolated(
					$"""
					SELECT *
					FROM platform.tenants
					WHERE id = {invitation.TenantId}
					FOR UPDATE
					""")
				.SingleOrDefaultAsync(cancellationToken);

			var tenantIsAvailable =
				tenant is not null &&
				(
					tenant.Status == TenantStatusEnum.Active ||
					(
						tenant.Status == TenantStatusEnum.Trial &&
						tenant.TrialEndsAt.HasValue &&
						tenant.TrialEndsAt.Value > now
					)
				);

			if (!tenantIsAvailable)
			{
				return AcceptInvitationResult.Failed(
					AcceptInvitationFailure.TenantUnavailable);
			}

			var role = await dbcontext.Roles
				.AsNoTracking()
				.SingleOrDefaultAsync(
					x =>
						x.Id == invitation.RoleId &&
						x.Scope == RoleScopeEnum.Tenant &&
						x.TenantId == invitation.TenantId &&
						!x.IsDeleted,
					cancellationToken);
					
			if(
				role is null ||
				!IsSupportedInvitationRole(role.Code))
			{
				return AcceptInvitationResult.Failed(
					AcceptInvitationFailure.RoleUnavailable);
			}

			if (!invitation.TargetUserId.HasValue)
			{
				return AcceptInvitationResult.Failed(
					AcceptInvitationFailure.TargetUserUnavailable);
			}

			var targetUser = await dbcontext.Users
				.SingleOrDefaultAsync(
					x => x.Id == invitation.TargetUserId.Value,
					cancellationToken);

			if(
				targetUser is null ||
				targetUser.IsDeleted ||
				targetUser.TenantId != invitation.TenantId ||
				targetUser.Status != UserStatusEnum.Inviting ||
				targetUser.EmailConfirmed ||
				string.IsNullOrWhiteSpace(targetUser.Email) ||
				!string.Equals(
					targetUser.Email,
					invitation.Email,
					StringComparison.OrdinalIgnoreCase))
			{
				return AcceptInvitationResult.Failed(
					AcceptInvitationFailure.TargetUserUnavailable);
			}

			if(await userManager.HasPasswordAsync(targetUser)){
				return AcceptInvitationResult.Failed(
					AcceptInvitationFailure.TargetUserUnavailable);
			}

			var targetUserHasActiveRole =
				await dbcontext.UserRoles
				.AsNoTracking()
				.AnyAsync(
					x =>
						x.UserId == targetUser.Id &&
						x.RevokedAt == null,
					cancellationToken);

			if (targetUserHasActiveRole)
			{
				return AcceptInvitationResult.Failed(
					AcceptInvitationFailure.ActiveRoleAlreadyExists);
			}

			if(role.Code == DbInitializer.TenantAdminRoleCode)
			{
				var tenantAdminAlreadyExists =
					await dbcontext.UserRoles
					.AsNoTracking()
					.AnyAsync(
						x =>
							x.Scope == RoleScopeEnum.Tenant &&
							x.TenantId == invitation.TenantId &&
							x.RoleId == role.Id &&
							x.RevokedAt == null,
						cancellationToken);

				if (tenantAdminAlreadyExists)
				{
					return AcceptInvitationResult.Failed(
						AcceptInvitationFailure.ActiveRoleAlreadyExists);
				}
			}

			var addPasswordResult =
				await userManager.AddPasswordAsync(
					targetUser,
					request.Password);

			if (!addPasswordResult.Succeeded)
			{
				return AcceptInvitationResult.PasswordRejected(
					addPasswordResult.Errors.Select(
						x => x.Description));
			}

			targetUser.Status = UserStatusEnum.Active;
			targetUser.EmailConfirmed = true;
			targetUser.UpdatedAt = now;

			var updateUserResult = await userManager.UpdateAsync(targetUser);

			EnsureIdentitySucceeded(
				updateUserResult,
				"Không thể kích hoạt tài khoản được mời.");

			var userRole = new UserRole
			{
				Id = Guid.NewGuid(),
				Scope = RoleScopeEnum.Tenant,
				TenantId = invitation.TenantId,
				UserId = targetUser.Id,
				RoleId = role.Id,
				GrantedByUserId = invitation.CreatedBy,
				GrantedAt = now,
				RevokedAt = null
			};

			dbcontext.UserRoles.Add(userRole);

			await dbcontext.SaveChangesAsync(cancellationToken);

			invitation.Status = InvitationStatusEnum.Accepted;

			invitation.AcceptedAt = now;

			await dbcontext.SaveChangesAsync(cancellationToken);

			await transaction.CommitAsync(cancellationToken);

			return AcceptInvitationResult.Success(
				new AcceptInvitationResponse(
					targetUser.Id,
					invitation.TenantId,
					tenant!.Slug,
					targetUser.Email!,
					now));
		}

		private static void EnsureIdentitySucceeded(
			IdentityResult result,
			string message)
		{
			if (result.Succeeded)
			{
				return;
			}

			var errors = string.Join(
				"; ",
				result.Errors.Select(
					x => $"{x.Code}: {x.Description}"));

			throw new InvalidOperationException($"{message} Chi tiết: {errors}");
		}

		private static bool IsSupportedInvitationRole(string roleCode)
		{
			return roleCode is
				DbInitializer.TenantAdminRoleCode or
				DbInitializer.ManagerRoleCode or
				DbInitializer.StaffRoleCode or
				DbInitializer.ResidentRoleCode;
		}
	}
}
