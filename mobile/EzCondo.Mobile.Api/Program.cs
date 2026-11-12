using EZCondoAPI.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using EzCondo.Mobile.Api;

var builder = WebApplication.CreateBuilder(args);
if (!builder.Environment.IsDevelopment())
    throw new InvalidOperationException("Mobile demo sessions require Development and a loopback-only local host.");
// Local demo sessions must never be exposed on a public listener.
builder.WebHost.UseUrls("http://127.0.0.1:5275");
var databasePath = builder.Configuration["MobileDatabase"]
    ?? Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "../.local/mobile.db"));
if (!File.Exists(databasePath))
    throw new FileNotFoundException("Run mobile/Start-Local.ps1 to prepare the local database.", databasePath);
builder.Services.AddDbContext<EZCondoDbContext>(options => options.UseSqlite(
    new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = databasePath, Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadWrite }.ToString()));
builder.Services.AddDataProtection().SetApplicationName("EzCondo.Mobile.Local")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Path.GetDirectoryName(databasePath)!, "keys")));
builder.Services.AddScoped<MobileAccess>();
builder.Services.AddProblemDetails();
var app = builder.Build();
app.UseExceptionHandler();
app.MapGet("/health", () => new { status = "ok", mode = "local-demo" });
MobileEndpoints.Map(app);
app.Run();
