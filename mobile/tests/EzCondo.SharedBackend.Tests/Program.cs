using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Entities.SchemaSecurity;
using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Infrastructure.Authentication;
using EzCondo.BackEndServer.Mobile;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

if (args is ["--cold-start", var localPath])
{
    await MobileLocalDatabase.StartAsync(Path.GetFullPath(localPath)).WaitAsync(TimeSpan.FromSeconds(15));
    Console.WriteLine("PASS local PostgreSQL startup returns without waiting for server pipe closure");
    return;
}

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");
var signingKey = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32));
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> {
    ["Jwt:Issuer"] = "test", ["Jwt:Audience"] = "test",
    ["Jwt:SigningKey"] = Convert.ToBase64String(signingKey.Key),
    ["Jwt:AccessTokenMinutes"] = "15" });
var postgres = Environment.GetEnvironmentVariable("EZCONDO_TEST_POSTGRES");
if (postgres is null)
    builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase("mobile-regression"));
else
{
    var connection = new NpgsqlConnectionStringBuilder(postgres);
    if (connection.Host != "127.0.0.1" || connection.Port != 55432 || connection.Database != "ezcondo_mobile_regression")
        throw new InvalidOperationException("PostgreSQL tests only accept the isolated regression database.");
    var source = new NpgsqlDataSourceBuilder(postgres);
    source.MapEnum<UserStatusEnum>("identity.user_status_enum");
    source.MapEnum<TenantStatusEnum>("platform.tenant_status_enum");
    source.MapEnum<RoleScopeEnum>("security.role_scope_enum");
    source.MapEnum<TenantInvoiceStatus>("tenant.tenant_invoice_status");
    var dataSource = source.Build();
    builder.Services.AddSingleton(dataSource);
    builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseNpgsql(dataSource));
}
builder.Services.AddIdentityCore<User>(o => { o.Lockout.MaxFailedAccessAttempts = 5; })
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllers().AddApplicationPart(typeof(MobileController).Assembly);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = "test", ValidAudience = "test", IssuerSigningKey = signingKey
    };
});
builder.Services.AddAuthorization(options => options.AddPolicy(AuthPolicies.PlatformScope,
    policy => policy.RequireAuthenticatedUser().RequireClaim(AppClaimTypes.Scope, "platform")));
builder.Services.AddMobileApi(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<EzCondo.BackEndServer.Services.SubscriptionPlans.ISubscriptionPlanService,
    EzCondo.BackEndServer.Services.SubscriptionPlans.SubscriptionPlanService>();
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.MapGet("/web-auth-probe", (HttpContext context) => context.User.Identity?.IsAuthenticated == true);
await app.StartAsync();
using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
var tenant = Guid.NewGuid(); var otherTenant = Guid.NewGuid();
var resident = Guid.NewGuid(); var neighbor = Guid.NewGuid(); var staff = Guid.NewGuid();
var ownInvoice = Guid.NewGuid(); var foreignInvoice = Guid.NewGuid();
Guid complaintId;
var unassignedComplaintId = Guid.NewGuid();
const string password = "Integration!2026test";
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (postgres is not null)
    {
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }
    db.Tenants.AddRange(new Tenant { Id = tenant, LegalName = "Condo A", Code = "A", Slug = "a", TaxCode = "1", Address = "A", Status = TenantStatusEnum.Active },
        new Tenant { Id = otherTenant, LegalName = "Condo B", Code = "B", Slug = "b", TaxCode = "2", Address = "B", Status = TenantStatusEnum.Active });
    var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    foreach (var (id, email, role) in new[] { (resident, "resident@test.local", "RESIDENT"), (neighbor, "neighbor@test.local", "RESIDENT"), (staff, "staff@test.local", "STAFF") })
    {
        var user = new User { Id = id, UserName = email, Email = email, FullName = role,
            TenantId = tenant, Status = UserStatusEnum.Active, LockoutEnabled = true };
        var created = await users.CreateAsync(user, password);
        Check(created.Succeeded, "fixture user created");
        var existingRole = db.Roles.Local.FirstOrDefault(x => x.TenantId == tenant && x.Code == role);
        var roleId = existingRole?.Id ?? Guid.NewGuid();
        if (existingRole is null) db.Roles.Add(new Role { Id = roleId, TenantId = tenant, Scope = RoleScopeEnum.Tenant, Code = role, Name = role });
        db.UserRoles.Add(new UserRole { Id = Guid.NewGuid(), UserId = id, RoleId = roleId, TenantId = tenant, Scope = RoleScopeEnum.Tenant });
    }
    var ownApartment = new Apartment { Id = Guid.NewGuid(), TenantId = tenant, ApartmentRenter = resident, ApartmentNo = "A-101", AreaM2 = 68 };
    var otherApartment = new Apartment { Id = Guid.NewGuid(), TenantId = tenant, ApartmentRenter = neighbor, ApartmentNo = "A-102", AreaM2 = 68 };
    db.Apartments.AddRange(ownApartment, otherApartment);
    db.Invoices.AddRange(
        new Invoice { Id = ownInvoice, TenantId = tenant, ApartmentId = ownApartment.Id, CreatedByStaff = staff, InvoiceNo = "OWN", TotalAmount = 123456.78m, Status = TenantInvoiceStatus.Published },
        new Invoice { Id = foreignInvoice, TenantId = tenant, ApartmentId = otherApartment.Id, CreatedByStaff = staff, InvoiceNo = "NEIGHBOR", TotalAmount = 999999, Status = TenantInvoiceStatus.Published },
        new Invoice { Id = Guid.NewGuid(), TenantId = tenant, ApartmentId = ownApartment.Id, CreatedByStaff = staff, InvoiceNo = "DRAFT", Status = TenantInvoiceStatus.Draft });
    var own = new Complaint { Id = Guid.NewGuid(), TenantId = tenant, CreatedByResident = resident, Title = "Own complaint", Message = "Own details", ReceivedByStaff = staff };
    complaintId = own.Id;
    var foreignResident = new User { Id = Guid.NewGuid(), UserName = "other@test.local", Email = "other@test.local", FullName = "Other tenant resident", TenantId = otherTenant, Status = UserStatusEnum.Active };
    Check((await users.CreateAsync(foreignResident, password)).Succeeded, "foreign tenant fixture user created");
    db.Complaints.AddRange(own,
        new Complaint { Id = unassignedComplaintId, TenantId = tenant, CreatedByResident = neighbor, Title = "Neighbor complaint", Message = "Private" },
        new Complaint { Id = Guid.NewGuid(), TenantId = otherTenant, CreatedByResident = foreignResident.Id, Title = "Other tenant", Message = "Private" });
    await db.SaveChangesAsync();
}
try
{
    Check((await client.GetAsync("/api/mobile/profile")).StatusCode == HttpStatusCode.Unauthorized, "anonymous requests rejected");
    var webToken = new JwtSecurityToken("test", "test", [new Claim(AppClaimTypes.Scope, "platform")],
        expires: DateTime.UtcNow.AddMinutes(5),
        signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(webToken));
    Check(await client.GetFromJsonAsync<bool>("/web-auth-probe"), "web token authenticates the web scheme");
    Check((await client.GetAsync("/api/platform/subscription-plans")).IsSuccessStatusCode, "platform web token can read subscription plans");
    Check((await client.GetAsync("/api/mobile/profile")).StatusCode == HttpStatusCode.Unauthorized, "web token is rejected by the mobile scheme");
    client.DefaultRequestHeaders.Authorization = null;
    Check((await client.PostAsJsonAsync("/api/mobile/sessions", new { email = "resident@test.local", password = "wrong" })).StatusCode == HttpStatusCode.Unauthorized, "invalid password rejected");
    Check((await client.PostAsJsonAsync("/api/mobile/sessions", new { accountId = resident })).StatusCode == HttpStatusCode.BadRequest, "demo impersonation rejected");
    var session = await client.PostAsJsonAsync("/api/mobile/sessions", new { email = "resident@test.local", password });
    session.EnsureSuccessStatusCode();
    var json = await session.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", json.GetProperty("token").GetString());
    Check(!await client.GetFromJsonAsync<bool>("/web-auth-probe"), "mobile token does not authenticate existing web routes");
    var invoices = await client.GetStringAsync("/api/mobile/invoices");
    Check(invoices.Contains("OWN") && !invoices.Contains("NEIGHBOR") && !invoices.Contains("DRAFT"), "resident sees own published invoices only");
    Check(invoices.Contains("123456.78"), "decimal invoice amounts preserved");
    var complaints = await client.GetStringAsync("/api/mobile/complaints");
    Check(complaints.Contains("Own complaint") && !complaints.Contains("Neighbor complaint") && !complaints.Contains("Other tenant"), "complaints isolated by resident and tenant");
    Check((await client.PatchAsJsonAsync($"/api/mobile/complaints/{complaintId}/status", new { expectedStatus = "in_progress", status = "resolved" })).StatusCode == HttpStatusCode.Forbidden, "resident cannot resolve complaints");
    Check((await client.PostAsJsonAsync("/api/mobile/complaints", new { title = "", description = "" })).StatusCode == HttpStatusCode.BadRequest, "invalid complaint rejected");
    var created = await client.PostAsJsonAsync("/api/mobile/complaints", new { title = "New complaint", description = "Real issue in my apartment" });
    Check(created.IsSuccessStatusCode, "resident can create complaint using shared model");
    var staffSession = await client.PostAsJsonAsync("/api/mobile/sessions", new { email = "staff@test.local", password });
    staffSession.EnsureSuccessStatusCode();
    json = await staffSession.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", json.GetProperty("token").GetString());
    complaints = await client.GetStringAsync("/api/mobile/complaints");
    Check(complaints.Contains("Own complaint") && !complaints.Contains("Neighbor complaint") && !complaints.Contains("Other tenant"), "staff sees assigned complaints only");
    Check((await client.PatchAsJsonAsync($"/api/mobile/complaints/{unassignedComplaintId}/status", new { expectedStatus = "in_progress", status = "resolved" })).StatusCode == HttpStatusCode.NotFound, "staff cannot resolve an unassigned complaint");
    Check((await client.PatchAsJsonAsync($"/api/mobile/complaints/{complaintId}/status", new { expectedStatus = "in_progress", status = "resolved" })).IsSuccessStatusCode, "assigned staff can resolve complaint");
    Check((await client.GetAsync("/WeatherForecast")).IsSuccessStatusCode, "existing web controller remains reachable");
    Check((await client.GetAsync("/api/platform/subscription-plans")).StatusCode == HttpStatusCode.Unauthorized, "mobile token cannot read platform subscription plans");
    Check((await client.GetAsync("/api/mobile/invoices")).StatusCode == HttpStatusCode.Forbidden, "staff cannot read resident invoices");
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Users.Single(x => x.Id == staff).IsDeleted = true;
    await db.SaveChangesAsync();
    Check((await client.GetAsync("/api/mobile/profile")).StatusCode == HttpStatusCode.Unauthorized, "deleted account token no longer accepted");
    client.DefaultRequestHeaders.Authorization = null;
    var lockedEmail = "neighbor@test.local";
    for (var attempt = 0; attempt < 5; attempt++)
        await client.PostAsJsonAsync("/api/mobile/sessions", new { email = lockedEmail, password = "wrong" });
    Check((await client.PostAsJsonAsync("/api/mobile/sessions", new { email = lockedEmail, password })).StatusCode == HttpStatusCode.Unauthorized,
        "password failures enforce Identity account lockout");
    HttpResponseMessage? limited = null;
    for (var attempt = 0; attempt < 35; attempt++)
    {
        limited?.Dispose();
        limited = await client.PostAsJsonAsync("/api/mobile/sessions", new { email = "missing@test.local", password = "wrong" });
        if (limited.StatusCode == HttpStatusCode.TooManyRequests) break;
    }
    Check(limited?.StatusCode == HttpStatusCode.TooManyRequests, "login attempts are rate limited");
    limited?.Dispose();
    Check((await client.GetAsync("/WeatherForecast")).IsSuccessStatusCode, "mobile login throttling does not throttle web routes");
}
finally { await app.StopAsync(); await app.DisposeAsync(); }
static void Check(bool ok, string message) { if (!ok) throw new Exception(message); Console.WriteLine("PASS " + message); }
