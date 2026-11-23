using System.Text;
using EzCondo.BackEndServer.Infrastructure.Authentication.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
                }
            };
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true, ValidIssuer = jwt.Issuer,
                ValidateAudience = true, ValidAudience = jwt.Audience + ".mobile",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30)
            };
        });
        services.AddAuthorization();
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("MobileLogin", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 30, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
        });
        services.AddScoped<MobileIdentity>();
        return services;
    }
}
