using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using EzCondo.BackEndServer.Services.SubscriptionPlans;
using EzCondo.BackEndServer.Infrastructure.Authentication.Options;
using EzCondo.BackEndServer.Infrastructure;
using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Services.Tenants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using EzCondo.BackEndServer.Infrastructure.Authentication;
using Microsoft.AspNetCore.Identity;
using EzCondo.BackEndServer.Services.Authentication;
using Microsoft.OpenApi.Models;
using EzCondo.BackEndServer.Infrastructure.Invitations;
using EzCondo.BackEndServer.Services.TenantAdminInvitations;

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
	.AddSignInManager()
	.AddEntityFrameworkStores<ApplicationDbContext>();

//Đăng ký TimeProvider
builder.Services.AddSingleton(TimeProvider.System);

//Đăng ký Invitation service
builder.Services.AddSingleton<
	IInvitationTokenService,
	InvitationTokenService>();

//Đăng ký JwtTokenService
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

//Đăng ký AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

//Đăng ký TenantAdminInvitationService
builder.Services.AddScoped<
	ITenantAdminInvitationService, TenantAdminInvitationService>();
//Đăng ký handler
builder.Services.AddScoped<JwtBearerEventsHandler>();

//Đăng ký PlatformAccounts
builder.Services.Configure<PlatformAccountsOptions>(
	builder.Configuration.GetSection(PlatformAccountsOptions.SectionName));

//Đăng ký Jwt
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
		x => IsValidSigningKey(x.SigningKey),
		"Jwt:SigningKey phải là Base64 hợp lệ và chứa ít nhất 32 byte.")
	.Validate(
		x =>
			x.AccessTokenMinutes is > 0 and <= 60,
		"Jwt:AccessTokenMinutes phải nằm trong khoảng 1 đến 60.")
	.ValidateOnStart();

//Đăng ký Authentication
var jwtOptions = builder.Configuration
	.GetRequiredSection(JwtOptions.SectionName)
	.Get<JwtOptions>()
	?? throw new InvalidOperationException("Không tìm thấy cấu hình Jwt.");

if (!IsValidSigningKey(jwtOptions.SigningKey))
{
	throw new InvalidOperationException(
		"Jwt:SigningKey phải là Base64 hợp lệ và chứa ít nhất 32 bytes.");
}

var signingKeyBytes = Convert.FromBase64String(jwtOptions.SigningKey);

builder.Services.AddAuthentication(x =>
{
	x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
	.AddJwtBearer(x =>
	{
		x.MapInboundClaims = false;

		x.EventsType = typeof(JwtBearerEventsHandler);

		x.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidIssuer = jwtOptions.Issuer,

			ValidateAudience = true,
			ValidAudience = jwtOptions.Audience,

			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes),

			ValidateLifetime = true,
			RequireExpirationTime = true,
			RequireSignedTokens = true,

			ClockSkew = TimeSpan.Zero,

			NameClaimType = "name",
			RoleClaimType = "role"
		};
	});

builder.Services.AddAuthorization(x =>
{
	x.AddPolicy(
		AuthPolicies.PlatformScope,
		policy =>
		{
			policy.RequireAuthenticatedUser();
			policy.RequireClaim(AppClaimTypes.Scope, "platform");
		});

	x.AddPolicy(
		AuthPolicies.TenantScope,
		policy =>
		{
			policy.RequireAuthenticatedUser();
			policy.RequireClaim(AppClaimTypes.Scope, "tenant");
		});
});

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

builder.Services.AddSwaggerGen(options =>
{
	options.AddSecurityDefinition(
		"Bearer",
		new OpenApiSecurityScheme
		{
			Name = "Authorization",
			Type = SecuritySchemeType.Http,
			Scheme = "bearer",
			BearerFormat = "JWT",
			In = ParameterLocation.Header,
			Description =
				"Nhập JWT access token. Không cần nhập tiền tố Bearer."
		});

	options.AddSecurityRequirement(
		new OpenApiSecurityRequirement
		{
			{
				new OpenApiSecurityScheme
				{
					Reference = new OpenApiReference
					{
						Type = ReferenceType.SecurityScheme,
						Id = "Bearer"
					}
				},
				Array.Empty<string>()
			}
		});
});

// Cho phép mở Swagger có chủ đích ở môi trường demo/staging mà không cần
// chuyển toàn bộ ứng dụng sang Development.
var swaggerEnabled = builder.Configuration.GetValue<bool>("Swagger:Enabled");

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
if (app.Environment.IsDevelopment() || swaggerEnabled)
{
	app.UseSwagger();
	app.UseSwaggerUI(options =>
	{
		options.SwaggerEndpoint(
			"/swagger/v1/swagger.json",
			"EzCondo API v1");
		options.RoutePrefix = "swagger";
	});
}

app.UseHttpsRedirection();

app.UseCors(ReactCorsPolicy);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

static bool IsValidSigningKey(string? value)
{
	if (string.IsNullOrWhiteSpace(value))
	{
		return false;
	}

	try
	{
		return Convert.FromBase64String(value).Length >= 32;
	}
	catch (FormatException)
	{
		return false;
	}
}