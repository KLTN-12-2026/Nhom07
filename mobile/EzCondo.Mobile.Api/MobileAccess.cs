using System.Security.Cryptography;
using EZCondoAPI.Data;
using EZCondoAPI.Models;
using EzCondo.Mobile.Contracts;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace EzCondo.Mobile.Api;

public sealed class MobileAccess(EZCondoDbContext db, IDataProtectionProvider protection)
{
    private readonly ITimeLimitedDataProtector protector = protection.CreateProtector("mobile-session-v1").ToTimeLimitedDataProtector();
    public UserAccount User { get; private set; } = null!;
    public StaffMember? Staff { get; private set; }
    public List<string> ApartmentCodes { get; private set; } = [];
    public bool IsResident => User.Role == "resident";

    public string Issue(string id) => protector.Protect(id, TimeSpan.FromHours(8));

    public async Task<bool> Authenticate(HttpContext context)
    {
        var header = context.Request.Headers.Authorization.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.Ordinal)) return false;
        string id;
        try { id = protector.Unprotect(header[7..], out _); }
        catch (CryptographicException) { return false; }
        return await Load(id);
    }

    public async Task<bool> Load(string id)
    {
        var user = await db.UserAccounts.AsNoTracking().Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Id == id && (u.Role == "resident" || u.Role == "staff"));
        if (user?.Tenant is null || user.Tenant.Status is not ("active" or "trial")) return false;
        User = user;
        // shortcut: legacy records reference apartment codes, migrate to TenantId + apartment FK before multi-tenant production.
        ApartmentCodes = await db.Apartments.Where(a => a.TenantId == user.TenantId
            && db.Apartments.Count(other => other.Code == a.Code) == 1
            && (user.Role == "staff" || a.Code == user.ApartmentCode)).Select(a => a.Code).ToListAsync();
        if (IsResident) return ApartmentCodes.Count == 1;
        Staff = await db.StaffMembers.AsNoTracking().SingleOrDefaultAsync(s => s.TenantId == user.TenantId && s.Email == user.Email);
        return Staff is not null && Staff.Status == "active" && await db.StaffMembers.CountAsync(s => s.Name == Staff.Name) == 1;
    }

    public Account Account() => new(User.Id, User.Name, User.Role, User.Tenant!.Name, User.ApartmentCode);
    public IQueryable<Complaint> Complaints() => db.Complaints.Where(c => ApartmentCodes.Contains(c.ApartmentCode)
        && (User.Role == "resident" ? c.ResidentName == User.Name
            : c.AssignedTo == Staff!.Name || c.AssignedTo == "KTV " + Staff!.Name));
}

public sealed class SessionFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var access = context.HttpContext.RequestServices.GetRequiredService<MobileAccess>();
        if (!await access.Authenticate(context.HttpContext)) return Results.Unauthorized();
        return await next(context);
    }
}
