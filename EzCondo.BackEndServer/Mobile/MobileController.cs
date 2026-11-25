using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using EzCondo.BackEndServer.Contracts.Authentication.Tenant;
using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaSecurity;
using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Infrastructure.Authentication;
using EzCondo.BackEndServer.Services.Authentication;
using EzCondo.Mobile.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

namespace EzCondo.BackEndServer.Mobile;

[ApiController]
[Route("api/mobile")]
[Authorize(AuthenticationSchemes = MobileApiRegistration.Scheme)]
public sealed class MobileController(ApplicationDbContext db, UserManager<User> users,
    MobileIdentity identity, IAuthService auth, TimeProvider timeProvider) : ControllerBase
{
    public sealed record Credentials([Required, EmailAddress, MaxLength(254)] string Email,
        [Required, MaxLength(256)] string Password);
    public sealed record ComplaintInput([Required, StringLength(200, MinimumLength = 5)] string Title,
        [Required, StringLength(4000, MinimumLength = 10)] string Description);

    [HttpGet("server"), AllowAnonymous]
    public async Task<IActionResult> Server(CancellationToken ct)
        => await db.Database.CanConnectAsync(ct)
            ? Ok(new ServerInfo("EzCondo.BackEndServer", "PostgreSQL"))
            : StatusCode(503, new { message = "Chưa kết nối được PostgreSQL của backend." });

    [HttpPost("sessions"), AllowAnonymous]
    [EnableRateLimiting("MobileLogin")]
    public async Task<IActionResult> Login(Credentials request, CancellationToken ct)
    {
        var email = request.Email.Trim();
        var user = await users.FindByEmailAsync(email);
        var context = user is null ? null : await identity.Find(user.Id, ct);
        if (context?.User.Tenant is not { } tenant)
            return InvalidCredentials();

        var sessionId = Guid.NewGuid();
        var result = await auth.LoginTenantAsync(new TenantLoginRequest(tenant.Slug, email, request.Password), ct, sessionId);
        if (result.Value is not { } session || session.UserId != context.Value.User.Id)
            return InvalidCredentials();

        var account = context.Value.Account;
        if (account.Role == "staff")
            account = account with { CanInviteResidents = await identity.CanInviteResidents(session.UserId, tenant.Id, ct) };

        var now = timeProvider.GetUtcNow();
        if (db.Database.IsRelational())
            await db.MobileSessions.Where(x => x.ExpiresAt <= now).ExecuteDeleteAsync(ct);
        db.MobileSessions.Add(new MobileSession { Id = sessionId, UserId = session.UserId, ExpiresAt = session.ExpiresAt });
        await db.SaveChangesAsync(ct);
        return Ok(new SessionResponse(session.AccessToken, account));
    }

    [HttpDelete("sessions/current")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(AppClaimTypes.JwtId), out var sessionId)) return Unauthorized();
        var session = await db.MobileSessions.FindAsync([sessionId], ct);
        if (session is not null)
        {
            db.MobileSessions.Remove(session);
            try { await db.SaveChangesAsync(ct); }
            catch (DbUpdateConcurrencyException) { return NoContent(); }
        }
        return NoContent();
    }

    private UnauthorizedObjectResult InvalidCredentials() => Unauthorized(new
        { message = "Thông tin đăng nhập không hợp lệ hoặc tài khoản không có quyền mobile." });

    private Task<(User User, Account Account)?> Current(CancellationToken ct) => identity.Find(User, ct);

    [HttpGet("profile")]
    public async Task<IActionResult> Profile(CancellationToken ct)
    {
        var current = await Current(ct); if (current is null) return Unauthorized();
        var (user, account) = current.Value;
        return Ok(new Profile(account, user.PhoneNumber ?? "", user.Email ?? "", user.Tenant!.Address,
            null, account.Role == "staff" ? "Nhân viên" : null, []));
    }

    private IQueryable<Invoice> ResidentInvoices(User user) => db.Invoices.AsNoTracking()
        .Where(x => x.TenantId == user.TenantId && !x.IsDeleted && !x.Apartment.IsDeleted
            && x.Apartment.TenantId == user.TenantId && x.Apartment.ApartmentRenter == user.Id
            && (x.Status == TenantInvoiceStatus.Published || x.Status == TenantInvoiceStatus.Unpaid || x.Status == TenantInvoiceStatus.Paid));

    private IQueryable<Complaint> VisibleComplaints(User user, Account account) => db.Complaints
        .Where(x => x.TenantId == user.TenantId && !x.IsDeleted
            && (account.Role == "resident" ? x.CreatedByResident == user.Id : x.ReceivedByStaff == user.Id));

    [HttpGet("overview")]
    public async Task<IActionResult> Overview(CancellationToken ct)
    {
        var current = await Current(ct); if (current is null) return Unauthorized();
        var (user, account) = current.Value;
        var open = await VisibleComplaints(user, account).CountAsync(x => x.ResolvedAt == null, ct);
        var due = account.Role == "resident" ? await ResidentInvoices(user)
            .Where(x => x.Status != TenantInvoiceStatus.Paid).SumAsync(x => x.TotalAmount, ct) : 0;
        return Ok(new Overview(open, 0, due, 0));
    }

    [HttpGet("invoices")]
    public async Task<IActionResult> Invoices(CancellationToken ct)
    {
        var current = await Current(ct); if (current is null) return Unauthorized();
        var (user, account) = current.Value; if (account.Role != "resident") return Forbid(MobileApiRegistration.Scheme);
        var rows = await ResidentInvoices(user).Include(x => x.InvoiceDetails).ThenInclude(x => x.TenantService)
            .OrderByDescending(x => x.BillingPeriod).ToListAsync(ct);
        return Ok(rows.Select(x => new InvoiceDto(x.Id.ToString(), x.InvoiceNo, x.BillingPeriod.ToString("yyyy-MM"),
            x.PaymentDeadlineAt.ToString("yyyy-MM-dd"), x.Status == TenantInvoiceStatus.Paid ? "paid"
                : x.PaymentDeadlineAt < DateTimeOffset.UtcNow ? "overdue" : "unpaid", x.TotalAmount,
            x.InvoiceDetails.Where(i => i.TenantId == user.TenantId).Select(i => new InvoiceLine(i.Description ?? i.TenantService.Name,
                i.Quantity, "", i.UnitPrice, i.Amount)).ToList())));
    }

    [HttpGet("contracts")]
    public async Task<IActionResult> Contracts(CancellationToken ct)
    {
        var current = await Current(ct); if (current is null) return Unauthorized();
        var (user, account) = current.Value; if (account.Role != "resident") return Forbid(MobileApiRegistration.Scheme);
        var rows = await db.Contracts.AsNoTracking().Where(x => x.TenantId == user.TenantId && !x.IsDeleted
            && !x.Apartment.IsDeleted && x.Apartment.TenantId == user.TenantId && x.Apartment.ApartmentRenter == user.Id)
            .OrderByDescending(x => x.StartsAt).ToListAsync(ct);
        return Ok(rows.Select(x => new ContractDto(x.ContractNo, "Hợp đồng căn hộ", x.StartsAt.ToString("yyyy-MM-dd"),
            x.EndsAt.ToString("yyyy-MM-dd"), x.EndsAt < DateTimeOffset.UtcNow ? "expired" : "active", null, null)));
    }

    private static ComplaintDto ToDto(Complaint x) => new(x.Id.ToString(), "PA-" + x.Id.ToString("N")[..8],
        x.Title, x.Message, "Phản ánh", "", Status(x), "", x.CreatedAt.ToString("O"), x.Staff?.FullName, null, []);
    private static string Status(Complaint x) => x.ResolvedAt != null ? "resolved" : x.ReceivedByStaff != null ? "in_progress" : "new";

    [HttpGet("complaints")]
    public async Task<IActionResult> Complaints(CancellationToken ct)
    {
        var current = await Current(ct); if (current is null) return Unauthorized();
        var rows = await VisibleComplaints(current.Value.User, current.Value.Account).AsNoTracking().Include(x => x.Staff)
            .OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
        return Ok(rows.Select(ToDto));
    }

    [HttpGet("complaints/{id:guid}")]
    public async Task<IActionResult> Complaint(Guid id, CancellationToken ct)
    {
        var current = await Current(ct); if (current is null) return Unauthorized();
        var row = await VisibleComplaints(current.Value.User, current.Value.Account).AsNoTracking().Include(x => x.Staff).FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? NotFound() : Ok(ToDto(row));
    }

    [HttpPost("complaints")]
    public async Task<IActionResult> CreateComplaint(ComplaintInput input, CancellationToken ct)
    {
        var current = await Current(ct); if (current is null) return Unauthorized();
        var (user, account) = current.Value; if (account.Role != "resident") return Forbid(MobileApiRegistration.Scheme);
        if (input.Title.Trim().Length < 5 || input.Description.Trim().Length < 10) return BadRequest(new { message = "Nội dung phản ánh chưa hợp lệ." });
        var row = new Complaint { Id = Guid.NewGuid(), TenantId = user.TenantId!.Value, CreatedByResident = user.Id,
            Title = input.Title.Trim(), Message = input.Description.Trim() };
        db.Complaints.Add(row); await db.SaveChangesAsync(ct);
        return Created($"/api/mobile/complaints/{row.Id}", ToDto(row));
    }

    [HttpPatch("complaints/{id:guid}/status")]
    public async Task<IActionResult> Resolve(Guid id, ChangeStatus input, CancellationToken ct)
    {
        var current = await Current(ct); if (current is null) return Unauthorized();
        var (user, account) = current.Value; if (account.Role != "staff") return Forbid(MobileApiRegistration.Scheme);
        if (input.Status != "resolved") return BadRequest(new { message = "Backend chỉ hỗ trợ đánh dấu đã xử lý." });
        var row = await VisibleComplaints(user, account).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (row is null) return NotFound();
        if (Status(row) != input.ExpectedStatus) return Conflict(new { message = "Phiếu đã thay đổi. Hãy tải lại." });
        row.ResolvedAt ??= DateTimeOffset.UtcNow; await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("service-catalog")]
    public async Task<IActionResult> Services(CancellationToken ct)
    {
        var current = await Current(ct); if (current is null) return Unauthorized();
        return Ok(await db.Services.AsNoTracking().Where(x => x.TenantId == current.Value.User.TenantId && !x.IsDeleted)
            .OrderBy(x => x.Name).Select(x => new ServiceCatalogDto(x.Id.ToString(), x.Code, x.Name, x.Price)).ToListAsync(ct));
    }

    [HttpGet("work-records")]
    public async Task<IActionResult> WorkRecords(CancellationToken ct)
    {
        var current = await Current(ct); if (current is null) return Unauthorized();
        var (user, account) = current.Value; if (account.Role != "staff") return Forbid(MobileApiRegistration.Scheme);
        var rows = await db.StaffTimeSheets.AsNoTracking().Where(x => x.TenantId == user.TenantId && x.StaffId == user.Id && !x.IsDeleted)
            .OrderByDescending(x => x.PayrollPeriod).ToListAsync(ct);
        return Ok(rows.Select(x => new WorkRecordDto(x.PayrollPeriod.ToString("yyyy-MM"), x.StandardWorkingDay,
            x.PaidLeaveDay, x.UnpaidLeaveDay, x.OvertimeHourNormal + x.OvertimeHourWeekend + x.OvertimeHourHoliday, x.Note)));
    }

    [HttpGet("payrolls")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Payrolls(CancellationToken ct)
    {
        var current = await Current(ct);
        if (current is null) return Unauthorized();
        var (user, account) = current.Value;
        if (account.Role != "staff") return Forbid(MobileApiRegistration.Scheme);

        var rows = await db.PayrollDetails.AsNoTracking()
            .Where(x => x.TenantId == user.TenantId && x.StaffReceive == user.Id
                && !x.Payroll.IsDeleted && x.Payroll.Status == PayrollStatusEnum.Published)
            .OrderByDescending(x => x.Payroll.PublishedAt ?? x.Payroll.CreatedAt)
            .ThenBy(x => x.Id)
            .Select(x => new PayrollDto(x.Payroll.Title, x.Payroll.PublishedAt, x.BaseSalary, x.Allowance,
                x.Deduction, x.BaseSalary + x.Allowance - x.Deduction))
            .ToListAsync(ct);
        return Ok(rows);
    }
}
