using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using EzCondo.BackEndServer.Services.SubscriptionPlans;
using EzCondo.BackEndServer.Infrastructure.Authentication.Options;
using EzCondo.BackEndServer.Infrastructure;
using System.Text;
using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Services.Tenants;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Đăng ký Cors
const string ReactCorsPolicy = "ReactApp";

var allowedOrigins = builder.Configuration.GetSection("Cors: AllowedOrigins").Get<string[]>()
	?? [];

builder.Services.AddCors(options =>
{
	options.AddPolicy(ReactCorsPolicy, policy =>
	{
		policy
		.WithOrigins(allowedOrigins)
		.AllowAnyHeader()
		.AllowAnyMethod();
	});
});

//Đăng ký UserManager<User>
builder.Services
	.AddIdentityCore<User>(options =>
	{
		options.User.RequireUniqueEmail = true;
		//Mật khẩu: tối thiểu 12 kí tự, ít nhất 1 chữ in hoa, 1 chữ thường, 1 kí tự đặc biệt.
		options.Password.RequiredLength = 12;
		options.Password.RequireUppercase = true;
		options.Password.RequireLowercase = true;
		options.Password.RequireDigit = true;
		options.Password.RequireNonAlphanumeric = true;

		options.Lockout.AllowedForNewUsers = true;
		options.Lockout.MaxFailedAccessAttempts = 5;
		options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
	})
	.AddEntityFrameworkStores<ApplicationDbContext>();

//Đăng ký TimeProvider
builder.Services.AddSingleton(TimeProvider.System);

//Đăng ký PlatformAccounts
builder.Services.Configure<PlatformAccountsOptions>(
	builder.Configuration.GetSection(PlatformAccountsOptions.SectionName));

builder.Services
	.AddOptions<JwtOptions>()
	.Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
	.Validate(
		x => !string.IsNullOrWhiteSpace(x.Issuer),
		"Jwt:Issuer không được để trống.")
	.Validate(
		x => !string.IsNullOrWhiteSpace(x.Audience),
		"Jwt:Audience không được để trống.")
	.Validate(
		x =>
			Encoding.UTF8.GetByteCount(x.SigningKey) >= 32,
		"Jwt:SigningKey phải có ít nhất 32 byte.")
	.Validate(
		x =>
			x.AccessTokenMinutes is > 0 and <= 60,
		"Jwt:AccessTokenMinutes phải nằm trong khoảng 1 đến 60.")
	.ValidateOnStart();

//Đăng ký ConnectionString
var connectionString = builder.Configuration
	.GetConnectionString("DefaultConnection")
	?? throw new InvalidOperationException("The 'DefaultConnection' connectionString is not found.");

//Cấu hình Data Source cho enums
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
//Bật tính năng Jsonb
dataSourceBuilder.EnableDynamicJson();
//Map enums
dataSourceBuilder.MapEnum<SubscriptionInvoiceStatus>("platform.subscription_invoice_status");
dataSourceBuilder.MapEnum<SubscriptionPaymentAttemptStatus>("platform.subscription_payment_attempt_status");
dataSourceBuilder.MapEnum<SubscriptionStatusEnum>("platform.subscription_status_enum");
dataSourceBuilder.MapEnum<TenantStatusEnum>("platform.tenant_status_enum");
dataSourceBuilder.MapEnum<RoleScopeEnum>("security.role_scope_enum");
dataSourceBuilder.MapEnum<UserStatusEnum>("identity.user_status_enum");
dataSourceBuilder.MapEnum<InvoicePaymentAttemptStatus>("tenant.invoice_payment_attempt_status");
dataSourceBuilder.MapEnum<InvitationStatusEnum>("tenant.invitation_status_enum");
dataSourceBuilder.MapEnum<PayrollDisbursementItemStatus>("tenant.payroll_disbursement_item_status");
dataSourceBuilder.MapEnum<PayrollDisbursementStatus>("tenant.payroll_disbursement_status");
dataSourceBuilder.MapEnum<PayrollStatusEnum>("tenant.payroll_status_enum");
dataSourceBuilder.MapEnum<TenantInvoiceStatus>("tenant.tenant_invoice_status");
dataSourceBuilder.MapEnum<PaymentRequestStatus>("payment.payment_request_status");
dataSourceBuilder.MapEnum<PaymentTransactionStatus>("payment.payment_transaction_status");
dataSourceBuilder.MapEnum<PayoutBatchStatus>("payment.payout_batch_status");
dataSourceBuilder.MapEnum<PayoutItemStatusEnum>("payment.payout_item_status_enum");
dataSourceBuilder.MapEnum<ProcessingStatusEnum>("payment.processing_status_enum");
dataSourceBuilder.MapEnum<ProviderOwnerScope>("payment.provider_owner_scope");

var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<ApplicationDbContext>(option =>
option.UseNpgsql(dataSource));

builder.Services.AddScoped<DbInitializer>();

//Đăng ký service cho SubscriptionPlan
builder.Services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
builder.Services.AddHttpContextAccessor();

//Đăng ký service cho Tenant
builder.Services.AddSingleton<
	ITenantIdentityGenerator,
	TenantIdentityGenerator>();

builder.Services.AddScoped<
	ITenantService,
	TenantService>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
	var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
	await initializer.InitializeAsync();
}

//Gọi seeder
await DataBaseSeeder.SeedPlatformAccountsAsync(
	app.Services,
	CancellationToken.None);



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(ReactCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
