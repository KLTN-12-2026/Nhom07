using System.Security.Claims;
using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.Mobile.Contracts;
using Microsoft.EntityFrameworkCore;

namespace EzCondo.BackEndServer.Mobile;

public sealed class MobileIdentity(ApplicationDbContext db)
{
    public async Task<(User User, Account Account)?> Find(Guid id, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking().Include(x => x.Tenant)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.Status == UserStatusEnum.Active, ct);
        if (user?.TenantId is not Guid tenant || user.Tenant is null
            || user.Tenant.Status is not (TenantStatusEnum.Active or TenantStatusEnum.Trial)
            || user.LockoutEnd > DateTimeOffset.UtcNow) return null;
        var roles = await db.UserRoles.AsNoTracking()
            .Where(x => x.UserId == id && x.TenantId == tenant && x.Scope == RoleScopeEnum.Tenant
                && x.RevokedAt == null && !x.Role.IsDeleted && x.Role.TenantId == tenant
                && x.Role.Scope == RoleScopeEnum.Tenant)
            .Select(x => x.Role.Code).ToListAsync(ct);
        var role = roles.Contains(DbInitializer.StaffRoleCode) ? "staff"
            : roles.Contains(DbInitializer.ResidentRoleCode) ? "resident" : null;
        if (role is null) return null;
        var apartment = role == "resident" ? await db.Apartments.AsNoTracking()
            .Where(x => x.TenantId == tenant && x.ApartmentRenter == id && !x.IsDeleted)
            .OrderBy(x => x.ApartmentNo).Select(x => x.ApartmentNo).FirstOrDefaultAsync(ct) : null;
        return (user, new Account(user.Id.ToString(), user.FullName, role, user.Tenant.LegalName, apartment));
    }

    public Task<(User User, Account Account)?> Find(ClaimsPrincipal principal, CancellationToken ct)
        => Guid.TryParse(principal.FindFirstValue("sub"), out var id) ? Find(id, ct)
            : Task.FromResult<(User, Account)?>(null);
}
