using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Infrastructure;
using EzCondo.BackEndServer.Infrastructure.Authentication;
using EzCondo.BackEndServer.Infrastructure.Authentication.Options;
using EzCondo.BackEndServer.Infrastructure.Email;
using EzCondo.BackEndServer.Infrastructure.Email.Options;
using EzCondo.BackEndServer.Infrastructure.Invitations;
using EzCondo.BackEndServer.Infrastructure.Invitations.Options;
using EzCondo.BackEndServer.Infrastructure.Payments.PayOS.Options;
using EzCondo.BackEndServer.Infrastructure.Subscriptions;
using EzCondo.BackEndServer.Infrastructure.Subscriptions.Options;
using EzCondo.BackEndServer.Services.Authentication;
using EzCondo.BackEndServer.Services.Invitations;
using EzCondo.BackEndServer.Services.SubscriptionInvoices;
using EzCondo.BackEndServer.Services.SubscriptionPlans;
using EzCondo.BackEndServer.Services.SubscriptionRenewals;
using EzCondo.BackEndServer.Services.TenantAdminInvitations;
using EzCondo.BackEndServer.Services.Tenants;
using EzCondo.BackEndServer.Mobile;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Microsoft.Extensions.Options;
using PayOS;
using EzCondo.BackEndServer.Infrastructure.Payments.PayOS;
using EzCondo.BackEndServer.Services.Payments.PayOS;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);
var mobileLocal = Environment.GetEnvironmentVariable("EZCONDO_MOBILE_LOCAL") == "1";
if (mobileLocal)
{
	var local = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "../mobile/.local"));
	builder.Configuration.AddJsonFile(Path.Combine(local, "shared-backend.json"), optional: false);
	await MobileLocalDatabase.StartAsync(local);
}

// Add services to the container.

//Đăng ký Cors
const string ReactCorsPolicy = "ReactApp";

var allowedOrigins = builder.Configuration
	.GetSection("Cors:AllowedOrigins")
	.Get<string[]>()
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

//Đăng ký SubscriptionRenewalReminderOptions
builder.Services
	.AddOptions<SubscriptionRenewalReminderOptions>()
	.Bind(
		builder.Configuration.GetSection(
			SubscriptionRenewalReminderOptions.SectionName))
	.Validate(
		x => x.CheckIntervalHours is >= 1 and <= 24,
		"CheckIntervalHours phải từ 1 đến 24 giờ.")
	.Validate(
		x => x.TrialReminderDays is >= 1 and <= 29,
		"TrialReminderDays phải từ 1 đến 29 ngày.")
	.Validate(
		x => x.PaidReminderMonths is >= 1 and <= 3,
		"PaidReminderMonths phải từ 1 đến 3 tháng.")
	.ValidateOnStart();

// Đăng ký cấu hình tích hợp PayOS
var payOSOptions = builder.Services
	.AddOptions<PayOSIntegrationOptions>()
	.Bind(
		builder.Configuration.GetSection(
			PayOSIntegrationOptions.SectionName))
	.Validate(
		x =>
			!string.IsNullOrWhiteSpace(x.ProviderAccountCode) &&
			x.ProviderAccountCode.Trim().Length <= 80,
		"PayOS:ProviderAccountCode phải có từ 1 đến 80 ký tự.")
	.Validate(
		x => !string.IsNullOrWhiteSpace(x.ClientId),
		"PayOS:ClientId không được để trống.")
	.Validate(
		x => !string.IsNullOrWhiteSpace(x.ApiKey),
		"PayOS:ApiKey không được để trống.")
	.Validate(
		x => !string.IsNullOrWhiteSpace(x.ChecksumKey),
		"PayOS:ChecksumKey không được để trống.")
	.Validate(
		x =>
			Uri.TryCreate(
				x.ReturnUrl,
				UriKind.Absolute,
				out var returnUri) &&
			(
				returnUri.Scheme == Uri.UriSchemeHttp ||
				returnUri.Scheme == Uri.UriSchemeHttps
			),
		"PayOS:ReturnUrl phải là URL HTTP/HTTPS tuyệt đối.")
	.Validate(
		x =>
			Uri.TryCreate(
				x.CancelUrl,
				UriKind.Absolute,
				out var cancelUri) &&
			(
				cancelUri.Scheme == Uri.UriSchemeHttp ||
				cancelUri.Scheme == Uri.UriSchemeHttps
			),
		"PayOS:CancelUrl phải là URL HTTP/HTTPS tuyệt đối.")
	.Validate(
		x => x.PaymentLinkMinutes is >= 5 and <= 60,
		"PayOS:PaymentLinkMinutes phải từ 5 đến 60 phút.")
	.Validate(
		x => x.TimeoutSeconds is >= 5 and <= 60,
		"PayOS:TimeoutSeconds phải từ 5 đến 60 giây.")
	.Validate(
		x => x.MaxRetries is >= 0 and <= 3,
		"PayOS:MaxRetries phải từ 0 đến 3.");
if (!mobileLocal) payOSOptions.ValidateOnStart();


// Đăng ký PayOS SDK client
builder.Services.AddSingleton<PayOSClient>(
	serviceProvider =>
	{
		var integrationOptions = serviceProvider
			.GetRequiredService<
				IOptions<PayOSIntegrationOptions>>()
			.Value;

		var sdkOptions =
			new global::PayOS.PayOSOptions
			{
				ClientId =
					integrationOptions.ClientId,

				ApiKey =
					integrationOptions.ApiKey,

				ChecksumKey =
					integrationOptions.ChecksumKey,

				TimeoutMs = checked(
					integrationOptions.TimeoutSeconds * 1000),

				MaxRetries =
					integrationOptions.MaxRetries,

				LogLevel =
					LogLevel.Warning
			};

		return new PayOSClient(sdkOptions);
	});

//Đăng ký PayOS gateway
builder.Services.AddSingleton<
	IPayOSGateway,
	PayOSGateway>();

//Đăng ký Invitation service
builder.Services.AddSingleton<
	IInvitationTokenService,
	InvitationTokenService>();

//đăng ký chức năng invitation hêt hạn
builder.Services
	.AddOptions<InvitationExpiryOptions>()
	.Bind(
		builder.Configuration.GetSection(
			InvitationExpiryOptions.SectionName))
	.Validate(
		x => x.CheckIntervalMinutes is >= 1 and <= 1440,
		"Invitations:Expiry:CheckIntervalMinutes không hợp lệ.")
	.ValidateOnStart();

builder.Services.AddScoped<
	IInvitationExpiryProcessor,
	InvitationExpiryProcessor>();

var invitationExpiryEnabled = builder.Configuration.GetValue<bool>(
	$"{InvitationExpiryOptions.SectionName}:Enabled");

if (!mobileLocal && invitationExpiryEnabled)
{
	builder.Services.AddHostedService<
		InvitationExpiryWorker>();
}

//Đăng ký luồng Tenant_admin từ Decline gia hạn Subscription
builder.Services
	.AddOptions<SubscriptionLifecycleOptions>()
	.Bind(
			builder.Configuration.GetSection(
				SubscriptionLifecycleOptions.SectionName))
	.Validate(
			x => x.CheckIntervalMinutes is >= 5 and <= 1440,
			"CheckIntervalMinutes không hợp lệ.")
	.Validate(
			x => x.ExpiryNoticeLeadHours is >= 1 and <= 72,
			"ExpiryNoticeLeadHours  không hợp lệ.")
	.Validate(
			x => x.RetryAfterMinutes is >= 5 and <= 120,
			"RetryAfterMinutes không hợp lệ.")
	.ValidateOnStart();

builder.Services.AddSingleton<
	ITenantBackgroundJobCoordinator,
	TenantBackgroundJobCoordinator>();

builder.Services.AddSingleton<
	ISubscriptionExpiryNoticeSender,
	MailkitSubscriptionExpiryNoticeSender>();

builder.Services.AddScoped<
	ISubscriptionLifecycleProcessor,
	SubscriptionLifecycleProcessor>();

//Đăng ký worker
var lifecycleEnabled = builder.Configuration.GetValue<bool>(
	$"{SubscriptionLifecycleOptions.SectionName}:Enabled");

if (!mobileLocal && lifecycleEnabled)
{
	builder.Services.AddHostedService<SubscriptionLifecycleWorker>();
}

//Đăng ký service webhook
builder.Services.AddScoped<
	IPayOSWebhookProcessingService,
	PayOSWebhookProcessingService>();

//Đăng ký JwtTokenService
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

//Đăng ký mailkit
var smtpOptions = builder.Services
	.AddOptions<SmtpOptions>()
	.Bind(
		builder.Configuration
		.GetSection(SmtpOptions.SectionName))
	.Validate(
		x => !string.IsNullOrWhiteSpace(x.Host),
		"Email:Smtp:Host không được để trống.")
	.Validate(
		x => x.Port is > 0 and <= 65535,
		"Email:Smtp:Port không hợp lệ.")
	.Validate(
		x => !string.IsNullOrWhiteSpace(x.Username),
		"Email:Smtp:Username không được để trống.")
	.Validate(
		x => !string.IsNullOrWhiteSpace(x.AppPassword),
		"Email:Smtp:AppPassword không được để trống.")
	.Validate(
		x => !string.IsNullOrWhiteSpace(x.FromAddress),
		"Email:Smtp:FromAddress không được để trống.")
	.Validate(
		x => !string.IsNullOrWhiteSpace(x.FromName),
		"Email:Smtp:FromName không được để trống.");
if (!mobileLocal) smtpOptions.ValidateOnStart();

//Đăng ký invitation link options
builder.Services
	.AddOptions<InvitationLinkOptions>()
	.Bind(
		builder.Configuration.GetSection(
			InvitationLinkOptions.SectionName))
	.Validate(
		x =>
			Uri.TryCreate(
				x.PortalBaseUrl,
				UriKind.Absolute,
				out var uri) &&
			(
				uri.Scheme == Uri.UriSchemeHttp ||
				uri.Scheme == Uri.UriSchemeHttps
			),
		"Invitations:PortalBaseUrl phải là URL HTTP/HTTPS hợp lệ.")
	.Validate(
		x => !string.IsNullOrWhiteSpace(x.AcceptPath),
		"Invitations:AcceptPath không được để trống.")
	.ValidateOnStart();

//Đăng ký sender
builder.Services.AddSingleton<
	IInvitationEmailSender,
	MailKitInvitationEmailSender>();

//Đăng ký AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

//Đăng ký TenantAdminInvitationService
builder.Services.AddScoped<
	ITenantAdminInvitationService, TenantAdminInvitationService>();

//Đăng ký InvitationAcceptanceService
builder.Services.AddScoped<IInvitationAcceptanceService, InvitationAcceptanceService>();

//Đăng ký renewal Service
builder.Services.AddScoped<
	ITenantSubscriptionRenewalService,
	TenantSubscriptionRenewalService>();

//Đăng ký pending review service
builder.Services.AddScoped<
	IPlatformSubscriptionRenewalService,
	PlatformSubscriptionRenewalService>();

//Đăng ký service tạo hóa đơn nháp cho subscription
builder.Services.AddScoped<
	ISubscriptionInvoiceService,
	SubscriptionInvoiceService>();

// Đăng ký service đọc hóa đơn subscription cho Tenant Admin
builder.Services.AddScoped<
	ITenantSubscriptionInvoiceService,
	TenantSubscriptionInvoiceService>();

// Đăng ký service tạo hoặc tái sử dụng PayOS payment link
builder.Services.AddScoped<
	ITenantSubscriptionPaymentLinkService,
	TenantSubscriptionPaymentLinkService>();

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

//Đăng ký service Auth chức năng Invitation
builder.Services.AddScoped<
	IAuthorizationHandler,
	PermissionAuthorizationHandler>();

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

	x.AddPolicy(
		AuthPolicies.PlatformTenantInvitationCreate,
		policy =>
		{
			policy.RequireAuthenticatedUser();

			policy.RequireClaim(
				AppClaimTypes.Scope,
				"platform");

			policy.AddRequirements(
				new PermissionRequirement(
					"platform.tenant_invitations.create"));
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
dataSourceBuilder.MapEnum<SubscriptionRenewalRequestStatus>("platform.subscription_renewal_request_status");
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

builder.Services.AddScoped<
	PlatformPayOSProviderAccountSeeder>();

builder.Services.AddScoped<DbInitializer>();

builder.Services.AddScoped<
	ISubscriptionRenewalReminderProcessor,
	SubscriptionRenewalReminderProcessor>();

var renewalReminderEnabled =
	builder.Configuration.GetValue<bool>(
		$"{SubscriptionRenewalReminderOptions.SectionName}:Enabled");

if (!mobileLocal && renewalReminderEnabled)
{
	builder.Services.AddHostedService<
		SubscriptionRenewalReminderWorker>();
}

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
builder.Services.AddMobileApi(builder.Configuration);
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
	var initializer =
		scope.ServiceProvider.GetRequiredService<DbInitializer>();

	await initializer.InitializeAsync(
		CancellationToken.None);

	if (!mobileLocal)
	{
		var payOSProviderAccountSeeder =
			scope.ServiceProvider.GetRequiredService<
				PlatformPayOSProviderAccountSeeder>();

		await payOSProviderAccountSeeder.SeedAsync(
			CancellationToken.None);
	}
}

//Gọi seeder
if (!mobileLocal)
{
	await DataBaseSeeder.SeedPlatformAccountsAsync(
		app.Services,
		CancellationToken.None);
}



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
app.UseRateLimiter();

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
