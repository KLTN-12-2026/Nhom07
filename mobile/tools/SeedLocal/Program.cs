using System.Security.Cryptography;
using System.Text.Json;
using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Entities.SchemaSecurity;
using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using EzCondo.BackEndServer.Data.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

if (args.Length is < 1 or > 2)
    throw new ArgumentException("Usage: SeedLocal <local-directory> [--change-staff-password | --change-resident-password]");

var emailToUpdate = args.ElementAtOrDefault(1) switch
{
    null => null,
    "--change-staff-password" => "staff@ezcondo.local",
    "--change-resident-password" => "resident@ezcondo.local",
    _ => throw new ArgumentException("Cờ đổi mật khẩu không hợp lệ.")
};
var local = Path.GetFullPath(args[0]);
using var config = JsonDocument.Parse(File.ReadAllText(Path.Combine(local, "shared-backend.json")).TrimStart('\uFEFF'));
var connection = new NpgsqlConnectionStringBuilder(config.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnection").GetString());
if (connection.Host != "127.0.0.1" || connection.Port != 55432 || connection.Database != "ezcondo_mobile_local")
    throw new InvalidOperationException("Seeder only accepts the isolated mobile local database.");
var source = new NpgsqlDataSourceBuilder(connection.ConnectionString);
source.MapEnum<UserStatusEnum>("identity.user_status_enum");
source.MapEnum<TenantStatusEnum>("platform.tenant_status_enum");
source.MapEnum<RoleScopeEnum>("security.role_scope_enum");
source.MapEnum<TenantInvoiceStatus>("tenant.tenant_invoice_status");
await using var dataSource = source.Build();
var services = new ServiceCollection().AddLogging(x => x.SetMinimumLevel(LogLevel.Warning).AddConsole());
services.AddDbContext<ApplicationDbContext>(x => x.UseNpgsql(dataSource));
services.AddIdentityCore<User>().AddEntityFrameworkStores<ApplicationDbContext>();
services.AddScoped<DbInitializer>();
await using var provider = services.BuildServiceProvider();
await using var scope = provider.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
var credentialFile = Path.Combine(local, "test-accounts.json");
if (emailToUpdate is not null)
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var account = await userManager.FindByEmailAsync(emailToUpdate)
        ?? throw new InvalidOperationException($"Không tìm thấy tài khoản local {emailToUpdate}.");

    Console.WriteLine($"Đổi mật khẩu cho {emailToUpdate}.");
    var currentPassword = ReadPassword("Mật khẩu hiện tại: ");
    var newPassword = ReadPassword("Mật khẩu mới: ");
    var confirmation = ReadPassword("Nhập lại mật khẩu mới: ");

    if (newPassword != confirmation)
        throw new InvalidOperationException("Hai lần nhập mật khẩu mới không khớp.");

    if (newPassword.Length < 12 || !newPassword.Any(char.IsUpper)
        || !newPassword.Any(char.IsLower) || !newPassword.Any(char.IsDigit)
        || !newPassword.Any(c => char.IsPunctuation(c) || char.IsSymbol(c)))
    {
        throw new InvalidOperationException(
            "Mật khẩu cần ít nhất 12 ký tự, có chữ hoa, chữ thường, số và ký tự đặc biệt.");
    }

    var result = await userManager.ChangePasswordAsync(account, currentPassword, newPassword);
    if (!result.Succeeded)
        throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

    Console.WriteLine($"Đã đổi mật khẩu cho {emailToUpdate}.");
    return;
}

static string ReadPassword(string prompt)
{
    if (Console.IsInputRedirected)
        throw new InvalidOperationException("Hãy chạy lệnh trong một cửa sổ PowerShell tương tác.");

    Console.Write(prompt);
    var value = new System.Text.StringBuilder();

    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            if (value.Length == 0)
            {
                Console.WriteLine("Bạn chưa nhập mật khẩu.");
                Console.Write(prompt);
                continue;
            }

            return value.ToString();
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            if (value.Length > 0)
            {
                value.Length--;
                Console.Write("\b \b");
            }
        }
        else if (!char.IsControl(key.KeyChar))
        {
            value.Append(key.KeyChar);
            Console.Write('*');
        }
    }
}
if (await db.Users.AnyAsync())
{
    Console.WriteLine("Local database already contains users; no existing users or passwords were changed.");
    return;
}
var password = "Local!" + Convert.ToHexString(RandomNumberGenerator.GetBytes(12)) + "a";
await using var transaction = await db.Database.BeginTransactionAsync();
var tenant = new Tenant
{
    Id = Guid.NewGuid(),
    Code = "MOBILE-LOCAL",
    Slug = "mobile-local",
    LegalName = "Chung cư EzCondo Local",
    Address = "Môi trường kiểm thử local",
    TaxCode = "LOCAL-TEST",
    Status = TenantStatusEnum.Active
};
db.Tenants.Add(tenant); await db.SaveChangesAsync();
await scope.ServiceProvider.GetRequiredService<DbInitializer>().SeedTenantDefaultCurrentTransactionAsync(tenant.Id);
var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
async Task<User> Create(string email, string name, string roleCode)
{
    var user = new User
    {
        Id = Guid.NewGuid(),
        UserName = email,
        Email = email,
        FullName = name,
        TenantId = tenant.Id,
        Status = UserStatusEnum.Active,
        EmailConfirmed = true,
        LockoutEnabled = true
    };
    var result = await users.CreateAsync(user, password);
    if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
    var role = await db.Roles.SingleAsync(x => x.TenantId == tenant.Id && x.Code == roleCode);
    db.UserRoles.Add(new UserRole
    {
        Id = Guid.NewGuid(),
        UserId = user.Id,
        RoleId = role.Id,
        TenantId = tenant.Id,
        Scope = RoleScopeEnum.Tenant,
        GrantedAt = DateTimeOffset.UtcNow
    });
    await db.SaveChangesAsync(); return user;
}
var staff = await Create("staff@ezcondo.local", "Nguyễn Văn Hùng", DbInitializer.StaffRoleCode);
var resident = await Create("resident@ezcondo.local", "Nguyễn Văn Minh", DbInitializer.ResidentRoleCode);
var apartment = new Apartment { Id = Guid.NewGuid(), TenantId = tenant.Id, ApartmentRenter = resident.Id, ApartmentNo = "A-1204", AreaM2 = 68.5m, Bedroom = 2 };
var service = new Service { Id = Guid.NewGuid(), TenantId = tenant.Id, Code = "QL", Name = "Phí quản lý chung cư", Price = 950000m, CreatedByManager = staff.Id };
db.Apartments.Add(apartment); db.Services.Add(service); await db.SaveChangesAsync();
var period = new DateTimeOffset(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);
var invoice = new Invoice
{
    Id = Guid.NewGuid(),
    TenantId = tenant.Id,
    ApartmentId = apartment.Id,
    InvoiceNo = "HD-LOCAL-" + period.ToString("yyyyMM"),
    CreatedByStaff = staff.Id,
    BillingPeriod = period,
    PaymentDeadlineAt = period.AddMonths(1),
    PublishedAt = DateTimeOffset.UtcNow,
    TotalAmount = 950000m,
    Status = TenantInvoiceStatus.Published
};
db.Invoices.Add(invoice);
db.InvoiceDetails.Add(new InvoiceDetail
{
    Id = Guid.NewGuid(),
    TenantId = tenant.Id,
    InvoiceId = invoice.Id,
    ServiceId = service.Id,
    Quantity = 1,
    UnitPrice = 950000m,
    Amount = 950000m,
    Description = service.Name
});
db.Contracts.Add(new Contract
{
    Id = Guid.NewGuid(),
    TenantId = tenant.Id,
    ApartmentId = apartment.Id,
    ImportedBy = staff.Id,
    ContractNo = "HDCH-LOCAL",
    StartsAt = period,
    EndsAt = period.AddYears(1)
});
db.Complaints.Add(new Complaint
{
    Id = Guid.NewGuid(),
    TenantId = tenant.Id,
    CreatedByResident = resident.Id,
    ReceivedByStaff = staff.Id,
    Title = "Kiểm tra vòi nước căn hộ",
    Message = "Vòi nước bếp chảy yếu, nhờ kỹ thuật kiểm tra."
});
db.StaffTimeSheets.Add(new StaffTimeSheet
{
    Id = Guid.NewGuid(),
    TenantId = tenant.Id,
    StaffId = staff.Id,
    CreatedByManager = staff.Id,
    PayrollPeriod = period,
    StandardWorkingDay = 26,
    PaidLeaveDay = 1,
    OvertimeHourNormal = 2,
    Note = "Dữ liệu kiểm thử local"
});
await db.SaveChangesAsync();
// Write credentials before committing so interrupted setup can roll back the accounts.
await File.WriteAllTextAsync(credentialFile, JsonSerializer.Serialize(new { resident = resident.Email, staff = staff.Email, password }, new JsonSerializerOptions { WriteIndented = true }));
await transaction.CommitAsync();
Console.WriteLine("Local test data created. Credentials: " + credentialFile);
