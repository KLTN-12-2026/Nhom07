using System.Security.Claims;
using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Infrastructure.Authentication;
using EzCondo.BackEndServer.Infrastructure.Authentication.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;

namespace EzCondo.BackEndServer.Mobile;

public static class MobileApiRegistration
{
    public const string Scheme = "EzCondoMobile";
    public static IServiceCollection AddMobileApi(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Missing Jwt configuration.");
        services.AddAuthentication().AddJwtBearer(Scheme, options =>
        {
            options.MapInboundClaims = false;
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Mobile credentials must not change the identity used by existing web routes.
                    if (!context.Request.Path.StartsWithSegments("/api/mobile")) context.NoResult();
                    return Task.CompletedTask;
                },
                OnTokenValidated = async context =>
                {
                    var services = context.HttpContext.RequestServices;
                    await services.GetRequiredService<JwtBearerEventsHandler>().TokenValidated(context);
                    if (context.Result?.Failure is not null) return;

                    if (!Guid.TryParse(context.Principal?.FindFirstValue(AppClaimTypes.JwtId), out var sessionId)
                        || !Guid.TryParse(context.Principal?.FindFirstValue(AppClaimTypes.Subject), out var userId)
                        || !await services.GetRequiredService<ApplicationDbContext>().MobileSessions.AsNoTracking()
                            .AnyAsync(x => x.Id == sessionId && x.UserId == userId, context.HttpContext.RequestAborted))
                        context.Fail("Phiên Android đã kết thúc.");
                }
            };
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true, ValidIssuer = jwt.Issuer,
                ValidateAudience = true, ValidAudience = jwt.Audience + ".mobile",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwt.SigningKey)),
                ValidateLifetime = true, ClockSkew = TimeSpan.Zero
            };
        });
        services.AddAuthorization();
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("MobileLogin", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 30, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
            options.AddPolicy("MobileInvitations", context => RateLimitPartition.GetFixedWindowLimiter(
                context.User.FindFirstValue(AppClaimTypes.Subject) ?? "anonymous",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 6, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
        });
        services.AddScoped<MobileIdentity>();
        return services;
    }
}
