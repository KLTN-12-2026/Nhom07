using System.IdentityModel.Tokens.Jwt;
using System.Text;
using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Infrastructure.Authentication;
using EzCondo.BackEndServer.Infrastructure.Authentication.Options;

namespace EzCondo.BackEndServer.UnitTests.Infrastructure.Authentication;

public class JwtTokenServiceTests
{
    private static readonly DateTimeOffset FixedNow =
        new(2026, 11, 25, 10, 0, 0, TimeSpan.Zero);

    private static JwtTokenService CreateService(int accessTokenMinutes = 15)
    {
        var options = new JwtOptions
        {
            Issuer = "EzCondo.Tests",
            Audience = "EzCondo.Tests",
            SigningKey = Convert.ToBase64String(
                Encoding.UTF8.GetBytes("0123456789abcdef0123456789abcdef")),
            AccessTokenMinutes = accessTokenMinutes
        };

        return new JwtTokenService(
            Microsoft.Extensions.Options.Options.Create(options),
            new FixedTimeProvider(FixedNow));
    }

    private static User NewUser(Guid? tenantId = null)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FullName = "Nguyen Van A",
            Email = "test@example.com",
            SecurityStamp = "security-stamp-123",
            TenantId = tenantId
        };
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    [Fact]
    public void CreateAccessToken_NullUser_ThrowsArgumentNullException()
    {
        var service = CreateService();

        Assert.Throws<ArgumentNullException>(() =>
            service.CreateAccessToken(
                null!,
                RoleScopeEnum.Platform,
                Array.Empty<string>()));
    }

    [Fact]
    public void CreateAccessToken_NullRoleCodes_ThrowsArgumentNullException()
    {
        var service = CreateService();

        Assert.Throws<ArgumentNullException>(() =>
            service.CreateAccessToken(
                NewUser(),
                RoleScopeEnum.Platform,
                null!));
    }

    [Fact]
    public void CreateAccessToken_MissingSecurityStamp_ThrowsInvalidOperationException()
    {
        var service = CreateService();
        var user = NewUser();
        user.SecurityStamp = null;

        Assert.Throws<InvalidOperationException>(() =>
            service.CreateAccessToken(
                user,
                RoleScopeEnum.Platform,
                Array.Empty<string>()));
    }

    [Fact]
    public void CreateAccessToken_MissingEmail_ThrowsInvalidOperationException()
    {
        var service = CreateService();
        var user = NewUser();
        user.Email = null;

        Assert.Throws<InvalidOperationException>(() =>
            service.CreateAccessToken(
                user,
                RoleScopeEnum.Platform,
                Array.Empty<string>()));
    }

    [Fact]
    public void CreateAccessToken_PlatformUserHasTenantId_ThrowsInvalidOperationException()
    {
        var service = CreateService();

        Assert.Throws<InvalidOperationException>(() =>
            service.CreateAccessToken(
                NewUser(Guid.NewGuid()),
                RoleScopeEnum.Platform,
                Array.Empty<string>()));
    }

    [Fact]
    public void CreateAccessToken_TenantUserWithoutTenantId_ThrowsInvalidOperationException()
    {
        var service = CreateService();

        Assert.Throws<InvalidOperationException>(() =>
            service.CreateAccessToken(
                NewUser(),
                RoleScopeEnum.Tenant,
                Array.Empty<string>()));
    }

    [Fact]
    public void CreateAccessToken_PlatformUser_ReturnsValidToken()
    {
        var service = CreateService(30);
        var user = NewUser();

        var result = service.CreateAccessToken(
            user,
            RoleScopeEnum.Platform,
            new[] { "Admin", "Manager" });

        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);

        Assert.Equal("EzCondo.Tests", token.Issuer);
        Assert.Contains("EzCondo.Tests", token.Audiences);
        Assert.Equal(
            user.Id.ToString(),
            token.Claims.Single(x => x.Type == AppClaimTypes.Subject).Value);
        Assert.Equal(
            "platform",
            token.Claims.Single(x => x.Type == AppClaimTypes.Scope).Value);
        Assert.Equal(
            user.Email,
            token.Claims.Single(x => x.Type == AppClaimTypes.Email).Value);
        Assert.Equal(
            user.SecurityStamp,
            token.Claims.Single(x => x.Type == AppClaimTypes.SecurityStamp).Value);
        Assert.Equal(FixedNow.UtcDateTime, token.ValidFrom);
        Assert.Equal(FixedNow.AddMinutes(30).UtcDateTime, token.ValidTo);
        Assert.Equal(FixedNow.AddMinutes(30), result.ExpiresAt);
        Assert.DoesNotContain(
            token.Claims,
            x => x.Type == AppClaimTypes.TenantId);
    }

    [Fact]
    public void CreateAccessToken_TenantUser_ReturnsTokenWithTenantAndSortedRoles()
    {
        var service = CreateService();
        var tenantId = Guid.NewGuid();

        var result = service.CreateAccessToken(
            NewUser(tenantId),
            RoleScopeEnum.Tenant,
            new[] { "Manager", "Admin", "Manager", "", "TenantAdmin" });

        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);

        Assert.Equal(
            "tenant",
            token.Claims.Single(x => x.Type == AppClaimTypes.Scope).Value);
        Assert.Equal(
            tenantId.ToString(),
            token.Claims.Single(x => x.Type == AppClaimTypes.TenantId).Value);

        var roles = token.Claims
            .Where(x => x.Type == AppClaimTypes.Role)
            .Select(x => x.Value)
            .ToList();

        Assert.Equal(new[] { "Admin", "Manager", "TenantAdmin" }, roles);
    }
}