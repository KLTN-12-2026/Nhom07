using EzCondo.BackEndServer.Contracts.TenantAdminInvitations;
using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Infrastructure.Authentication;
using EzCondo.BackEndServer.Infrastructure.Invitations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EzCondo.BackEndServer.Services.TenantAdminInvitations
{
	public sealed class TenantAdminInvitationService(
		ApplicationDbContext dbcontext,
		UserManager<User> userManager,
		IInvitationTokenService invitationTokenService,
		TimeProvider timeProvider,
		IHttpContextAccessor httpContextAccessor)
		: ITenantAdminInvitationService
	{
		private static readonly TimeSpan InvitationLifetime =
			TimeSpan.FromHours(48);

		public async Task<CreateTenantAdminInvitationResult> CreateAsync(
			Guid tenantId,
			CreateTenantAdminInvitationRequest request,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(request);

			var email = request.Email.Trim();

			var fullName = request.FullName.Trim();

			var citizenId = string.IsNullOrWhiteSpace(request.CitizenId)
				? null
				: request.CitizenId.Trim();

			var platformAdminId = GetCurrentUserId();

			var now = timeProvider.GetUtcNow();

			await using var transaction = await dbcontext
				.Database
				.BeginTransactionAsync(
					cancellationToken);

			var tenant = await dbcontext
				.Tenants
				.FromSqlInterpolated(
					$"""
					SELECT *
					FROM platform.tenants
					WHERE id = {tenantId}
					FOR UPDATE
					""")
				.SingleOrDefaultAsync(cancellationToken);
			if(tenant is null)
			{
				return CreateTenantAdminInvitationResult.Failed(
					CreateTenantAdminInvitationFailure.TenantNotFound);
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
				return CreateTenantAdminInvitationResult.Failed(
					CreateTenantAdminInvitationFailure.TenantUnavailable);
			}

			var tenantAdminRole = await dbcontext
				.Roles
				.SingleOrDefaultAsync(
					x =>
						x.Scope == RoleScopeEnum.Tenant &&
						x.TenantId == tenantId &&
						x.Code == DbInitializer.TenantAdminRoleCode &&
						!x.IsDeleted,
					cancellationToken);

			if(tenantAdminRole is null)
			{
				return CreateTenantAdminInvitationResult.Failed(
					CreateTenantAdminInvitationFailure.TenantAdminRoleNotFound);
			}

			await ExpireOldInvitationsAsync(
				tenantId,
				tenantAdminRole.Id,
				now,
				cancellationToken);

			var tenantAdminAlreadyExists =
				await dbcontext.UserRoles
					.AsNoTracking()
					.AnyAsync(
						x =>
							x.Scope == RoleScopeEnum.Tenant &&
							x.TenantId == tenantId &&
							x.RoleId == tenantAdminRole.Id &&
							x.RevokedAt == null,
						cancellationToken);

			if (tenantAdminAlreadyExists)
			{
				return CreateTenantAdminInvitationResult.Failed(
					CreateTenantAdminInvitationFailure
					.TenantAdminAlreadyExists);
			}

			var activeInvitationExists =
				await dbcontext.Invitations
				.AsNoTracking()
				.AnyAsync(
					x =>
						x.TenantId == tenantId &&
						x.RoleId == tenantAdminRole.Id &&
						!x.IsDeleted &&
						x.ExpiresAt > now &&
						(
							x.Status == InvitationStatusEnum.PendingDelivery ||
							x.Status == InvitationStatusEnum.Sent
						),
					cancellationToken);

			if (activeInvitationExists)
			{
				return CreateTenantAdminInvitationResult.Failed(
					CreateTenantAdminInvitationFailure
					.ActiveInvitationAlreadyExists);
			}

			var targetUser = await userManager.FindByEmailAsync(email);

			if(targetUser is null)
			{
				targetUser = new User
				{
					Id = Guid.NewGuid(),
					UserName = email,
					Email = email,
					FullName = fullName,
					CitizenId = citizenId,
					TenantId = tenantId,
					Status = UserStatusEnum.Inviting,
					EmailConfirmed = false,
					LockoutEnabled = true,
					CreatedAt = now
				};

				var createUserResult = await userManager
					.CreateAsync(targetUser);

				if (!createUserResult.Succeeded)
				{
					if (HasDuplicateIdentityError(createUserResult))
					{
						return CreateTenantAdminInvitationResult.Failed(
							CreateTenantAdminInvitationFailure
							.EmailAlreadyAssignedToAnotherAccount);
					}

					ThrowIdentityFailure(
						createUserResult,
						$"Không thể tạo tài khoản được mời '{email}'");
				}
			}
			else
			{
				var canReuseInvitingUser =
					targetUser.TenantId == tenantId &&
					targetUser.Status == UserStatusEnum.Inviting &&
					!targetUser.IsDeleted &&
					!await userManager.HasPasswordAsync(targetUser);

				if (!canReuseInvitingUser)
				{
					return CreateTenantAdminInvitationResult.Failed(
						CreateTenantAdminInvitationFailure
						.EmailAlreadyAssignedToAnotherAccount);
				}

				var hasActiveRole = await dbcontext.UserRoles
					.AsNoTracking()
					.AnyAsync(
						x =>
							x.UserId == targetUser.Id &&
							x.RevokedAt == null,
						cancellationToken);

				if (hasActiveRole)
				{
					return CreateTenantAdminInvitationResult.Failed(
						CreateTenantAdminInvitationFailure
						.EmailAlreadyAssignedToAnotherAccount);
				}

				targetUser.FullName = fullName;
				targetUser.CitizenId = citizenId ?? targetUser.CitizenId;

				var updateUserResult = await userManager.UpdateAsync(targetUser);

				EnsureIdentitySucceeded(
					updateUserResult,
					$"Không thể cập nhật tài khoản được mời '{email}'");
			}

			var generatedToken = invitationTokenService.Generate();

			var invitation = new Invitation
			{
				Id = Guid.NewGuid(),
				TenantId = tenantId,
				RoleId = tenantAdminRole.Id,
				TargetUserId = targetUser.Id,
				CreatedBy = platformAdminId,
				Email = email,
				TokenHash = generatedToken.Hash,
				Status = InvitationStatusEnum.PendingDelivery,
				CreatedAt = now,
				ExpiresAt = now.Add(InvitationLifetime),
				EmailSentAt = null,
				LastEmailAttemptAt = null,
				EmailSendAttemptCount = 0,
				LastEmailErrorCode = null
			};

			dbcontext.Invitations.Add(invitation);

			await dbcontext.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);

			return CreateTenantAdminInvitationResult.Success(
				new TenantAdminInvitationResponse(
					invitation.Id,
					tenantId,
					targetUser.Id,
					email,
					targetUser.FullName,
					invitation.Status,
					invitation.ExpiresAt,
					invitation.EmailSentAt,
					invitation.EmailSendAttemptCount));
		}

		private static void ThrowIdentityFailure(
			IdentityResult result,
			string message)
		{
			var errors = string.Join(
				"; ",
				result.Errors.Select(
					x => $"{x.Code}: {x.Description}"));

			throw new InvalidOperationException(
				$"{message}. Chi tiết: {errors}");
		}

		private static void EnsureIdentitySucceeded(
			IdentityResult result, 
			string message)
		{
			if (result.Succeeded)
			{
				return;
			}
			ThrowIdentityFailure(result, message);
		}

		private static bool HasDuplicateIdentityError(IdentityResult result)
		{
			return result.Errors.Any(
				x =>
					x.Code == "DuplicateEmail" ||
					x.Code == "DuplicateUserName");
		}

		private async Task ExpireOldInvitationsAsync(
			Guid tenantId, 
			Guid roleId, 
			DateTimeOffset now, 
			CancellationToken cancellationToken)
		{
			await dbcontext.Invitations
				.Where(
					x =>
						x.TenantId == tenantId &&
						x.RoleId == roleId &&
						!x.IsDeleted &&
						x.ExpiresAt <= now &&
						(
							x.Status == InvitationStatusEnum.PendingDelivery ||
							x.Status == InvitationStatusEnum.Sent
						))
				.ExecuteUpdateAsync(
					setter => setter.SetProperty(
						x => x.Status,
						InvitationStatusEnum.Expired),
					cancellationToken);
		}

		private Guid GetCurrentUserId()
		{
			var value = httpContextAccessor.HttpContext?.User
				.FindFirstValue(AppClaimTypes.Subject);

			return Guid.TryParse(value, out var userId)
				? userId
				: throw new UnauthorizedAccessException(
					"Không xác định được tài khoản Platform hiện tại.");
		}
	}
}
