using System.ComponentModel.DataAnnotations;
using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Infrastructure.Email;
using EzCondo.BackEndServer.Infrastructure.Invitations;
using EzCondo.BackEndServer.Infrastructure.Invitations.Options;
using EzCondo.Mobile.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace EzCondo.BackEndServer.Mobile;

[ApiController]
[Route("api/mobile/resident-invitations")]
[Authorize(AuthenticationSchemes = MobileApiRegistration.Scheme)]
public sealed class MobileResidentInvitationsController(ApplicationDbContext db, MobileIdentity identity,
    UserManager<User> users, IInvitationTokenService tokens, IOptions<InvitationLinkOptions> links,
    TimeProvider clock, ILogger<MobileResidentInvitationsController> logger) : ControllerBase
{
    public sealed record Input([Required, StringLength(150)] string FullName,
        [Required, EmailAddress, StringLength(254)] string Email);

    [HttpPost]
    [EnableRateLimiting("MobileInvitations")]
    public async Task<IActionResult> Invite(Input input, CancellationToken ct)
    {
        var current = await identity.Find(User, ct);
        if (current is null) return Unauthorized();
        var (staff, account) = current.Value;
        var tenantId = staff.TenantId!.Value;
        if (account.Role != "staff" || !await identity.CanInviteResidents(staff.Id, tenantId, ct))
            return Forbid(MobileApiRegistration.Scheme);
        if (!Uri.TryCreate(links.Value.MobileAcceptanceUrl, UriKind.Absolute, out var url)
            || (url.Scheme != Uri.UriSchemeHttps && !(url.Scheme == Uri.UriSchemeHttp && url.IsLoopback))
            || url.UserInfo.Length != 0 || url.Fragment.Length != 0)
            return StatusCode(503, new { message = "Máy chủ chưa cấu hình liên kết kích hoạt cư dân." });

        var email = input.Email.Trim();
        var fullName = input.FullName.Trim();
        if (fullName.Length == 0) return BadRequest(new { message = "Vui lòng nhập họ tên cư dân." });
        var roleId = await db.Roles.Where(x => x.TenantId == tenantId && x.Scope == RoleScopeEnum.Tenant
                && x.Code == DbInitializer.ResidentRoleCode && !x.IsDeleted)
            .Select(x => (Guid?)x.Id).SingleOrDefaultAsync(ct);
        if (roleId is null) return Conflict(new { message = "Chung cư chưa có vai trò cư dân." });

        var now = clock.GetUtcNow();
        var token = tokens.Generate();
        Invitation invitation;
        string recipientName;
        await using (var transaction = await db.Database.BeginTransactionAsync(ct))
        {
            try
            {
                var target = await users.FindByEmailAsync(email);
                if (target is null)
                {
                    target = new User { Id = Guid.NewGuid(), UserName = email, Email = email, FullName = fullName,
                        TenantId = tenantId, Status = UserStatusEnum.Inviting, LockoutEnabled = true };
                    if (!(await users.CreateAsync(target)).Succeeded) return EmailUnavailable();
                }
                else if (target.TenantId != tenantId || target.IsDeleted || target.Status != UserStatusEnum.Inviting
                    || target.EmailConfirmed || await users.HasPasswordAsync(target)
                    || await db.UserRoles.AnyAsync(x => x.UserId == target.Id && x.RevokedAt == null, ct))
                {
                    return EmailUnavailable();
                }

                var active = await db.Invitations.FromSqlInterpolated($"""
                    SELECT * FROM tenant.invitations
                    WHERE tenant_id = {tenantId} AND target_user_id = {target.Id}
                      AND NOT is_deleted AND status IN ('pending_delivery', 'sent')
                    FOR UPDATE
                    """).ToListAsync(ct);
                foreach (var item in active.Where(x => x.ExpiresAt <= now && x.IsMobileResidentInvitation))
                    item.Status = InvitationStatusEnum.Expired;

                var pending = active.Where(x => x.Status != InvitationStatusEnum.Expired).ToList();
                if (pending.Count > 1 || pending.Any(x => !x.IsMobileResidentInvitation || x.RoleId != roleId
                    || x.Status == InvitationStatusEnum.Sent
                    || (x.LastEmailErrorCode is null && x.LastEmailAttemptAt > now.AddMinutes(-2))))
                    return Conflict(new { message = "Địa chỉ này đã có lời mời đang hoạt động hoặc đang được gửi." });

                await db.SaveChangesAsync(ct);
                invitation = pending.SingleOrDefault() ?? new Invitation { Id = Guid.NewGuid(), TenantId = tenantId,
                    RoleId = roleId.Value, TargetUserId = target.Id, CreatedBy = staff.Id, Email = target.Email!,
                    CreatedAt = now, IsMobileResidentInvitation = true };
                if (pending.Count == 0) db.Invitations.Add(invitation);
                invitation.TokenHash = token.Hash;
                invitation.Status = InvitationStatusEnum.PendingDelivery;
                invitation.ExpiresAt = now.AddHours(48);
                invitation.LastEmailAttemptAt = now;
                invitation.EmailSendAttemptCount++;
                invitation.LastEmailErrorCode = null;
                recipientName = target.FullName;
                await db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch (DbUpdateException error) when (error.InnerException is PostgresException
                { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                return Conflict(new { message = "Lời mời hoặc tài khoản đã được tạo. Vui lòng kiểm tra lại." });
            }
        }

        try
        {
            // Resolve SMTP only after authorization so a missing SMTP setup cannot block other mobile APIs.
            var sender = HttpContext.RequestServices.GetRequiredService<IInvitationEmailSender>();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
            await sender.SendAsync(new InvitationEmailMessage(invitation.Email, recipientName, staff.Tenant!.LegalName,
                staff.Tenant.Slug, InvitationRecipientType.Resident, token.Value, invitation.ExpiresAt, url), timeout.Token);
            invitation.Status = InvitationStatusEnum.Sent;
            invitation.EmailSentAt = clock.GetUtcNow();
        }
        catch (Exception error)
        {
            invitation.LastEmailErrorCode = "delivery_failed";
            logger.LogWarning("Không gửi được lời mời {InvitationId} ({ErrorType}).", invitation.Id, error.GetType().Name);
            await db.SaveChangesAsync(CancellationToken.None);
            return StatusCode(503, new { message = "Chưa gửi được email. Vui lòng thử lại hoặc liên hệ quản trị." });
        }

        await db.SaveChangesAsync(CancellationToken.None);
        return StatusCode(201, new ResidentInvitationDto(invitation.Id, invitation.ExpiresAt));
    }

    private ConflictObjectResult EmailUnavailable() => Conflict(new
        { message = "Không thể gửi lời mời cho địa chỉ email này." });
}
