using EzCondo.BackEndServer.Contracts.Authentication.Platform;
using EzCondo.BackEndServer.Contracts.Authentication.Tenant;
using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Entities.SchemaSecurity;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Infrastructure.Authentication;
using EzCondo.BackEndServer.Services.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EzCondo.BackEndServer.UnitTests.Services.Authentication;

public class AuthServiceLoginSuccessTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 11, 25, 0, 0, 0, TimeSpan.Zero);

    private readonly DbContextOptions<ApplicationDbContext> _options =
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private readonly Mock<UserManager<User>> _userManager;
    private readonly Mock<SignInManager<User>> _signInManager;
    private readonly Mock<IJwtTokenService> _jwt = new();

    public AuthServiceLoginSuccessTests()
    {
        var store = new Mock<IUserStore<User>>();
        _userManager = new Mock<UserManager<User>>(
            store.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        _signInManager = new Mock<SignInManager<User>>(
            _userManager.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<User>>().Object,
            null!, null!, null!, null!);

        _signInManager
            .Setup(x => x.CheckPasswordSignInAsync(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        _jwt
            .Setup(x => x.CreateAccessToken(
                It.IsAny<User>(),
                It.IsAny<RoleScopeEnum>(),
                It.IsAny<IReadOnlyCollection<string>>()))
            .Returns(new AccessTokenResult("token", Now.AddMinutes(15)));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private ApplicationDbContext NewDb() => new(_options);

    private AuthService NewService(ApplicationDbContext db)
    {
        return new AuthService(
            _userManager.Object,
            _signInManager.Object,
            db,
            _jwt.Object,
            new FixedTimeProvider(Now));
    }

    private static User NewUser(Guid? tenantId = null)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            UserName = "user@example.com",
            Email = "user@example.com",
            FullName = "Nguyen Van A",
            TenantId = tenantId,
            Status = UserStatusEnum.Active,
            EmailConfirmed = true,
            IsDeleted = false
        };
    }

    private void ArrangeUser(User user)
    {
        _userManager
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
    }

    private async Task<Tenant> SeedActiveTenantAsync()
    {
        await using var db = NewDb();

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Code = "TEN-A",
            Slug = "chung-cu-a",
            LegalName = "Chung cu A",
            TaxCode = "TAX-A",
            Address = "12 Nguyen Hue, Da Nang",
            Status = TenantStatusEnum.Active
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();
        return tenant;
    }

    // Tạo Role và gán cho user qua UserRole.
    private async Task SeedRoleAsync(
        User user,
        RoleScopeEnum roleScope,
        Guid? roleTenantId,
        string code,
        bool roleDeleted = false,
        bool revoked = false,
        RoleScopeEnum? userRoleScope = null,
        Guid? userRoleTenantId = null)
    {
        await using var db = NewDb();

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Scope = roleScope,
            TenantId = roleTenantId,
            Code = code,
            Name = code,
            IsDeleted = roleDeleted
        };

        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            Scope = userRoleScope ?? roleScope,
            TenantId = userRoleTenantId ?? roleTenantId,
            UserId = user.Id,
            RoleId = role.Id,
            GrantedAt = Now.AddDays(-1),
            RevokedAt = revoked ? Now.AddHours(-1) : null
        };

        db.Roles.Add(role);
        db.UserRoles.Add(userRole);
        await db.SaveChangesAsync();
    }

    private void VerifyTokenCreated(
        User user,
        RoleScopeEnum scope,
        params string[] expectedRoles)
    {
        _jwt.Verify(
            x => x.CreateAccessToken(
                user,
                scope,
                It.Is<IReadOnlyCollection<string>>(
                    roles => roles.SequenceEqual(expectedRoles))),
            Times.Once);
    }

    private void VerifyTokenNeverCreated()
    {
        _jwt.Verify(
            x => x.CreateAccessToken(
                It.IsAny<User>(),
                It.IsAny<RoleScopeEnum>(),
                It.IsAny<IReadOnlyCollection<string>>()),
            Times.Never);
    }

    // ---------- LoginPlatformAsync ----------

    [Fact]
    public async Task LoginPlatformAsync_AdminWithActivePlatformRole_SucceedsAndCreatesPlatformToken()
    {
        var user = NewUser();
        ArrangeUser(user);
        await SeedRoleAsync(
            user,
            RoleScopeEnum.Platform,
            null,
            DbInitializer.PlatformAdminRoleCode);
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginPlatformAsync(
            new PlatformLoginRequest("user@example.com", "Passw0rd!"));

        Assert.True(result.IsSuccess);
        Assert.Equal(LoginFailure.None, result.Failure);
        Assert.NotNull(result.Value);
        VerifyTokenCreated(
            user,
            RoleScopeEnum.Platform,
            DbInitializer.PlatformAdminRoleCode);
    }

    [Fact]
    public async Task LoginPlatformAsync_RoleRevoked_ReturnsNoActiveRole()
    {
        var user = NewUser();
        ArrangeUser(user);
        await SeedRoleAsync(
            user,
            RoleScopeEnum.Platform,
            null,
            DbInitializer.PlatformAdminRoleCode,
            revoked: true);
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginPlatformAsync(
            new PlatformLoginRequest("user@example.com", "Passw0rd!"));

        Assert.Equal(LoginFailure.NoActiveRole, result.Failure);
        VerifyTokenNeverCreated();
    }

    [Fact]
    public async Task LoginPlatformAsync_RoleDeleted_ReturnsNoActiveRole()
    {
        var user = NewUser();
        ArrangeUser(user);
        await SeedRoleAsync(
            user,
            RoleScopeEnum.Platform,
            null,
            DbInitializer.PlatformAdminRoleCode,
            roleDeleted: true);
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginPlatformAsync(
            new PlatformLoginRequest("user@example.com", "Passw0rd!"));

        Assert.Equal(LoginFailure.NoActiveRole, result.Failure);
        VerifyTokenNeverCreated();
    }

    [Fact]
    public async Task LoginPlatformAsync_RoleIsNotPlatformAdmin_ReturnsNoActiveRole()
    {
        var user = NewUser();
        ArrangeUser(user);
        await SeedRoleAsync(
            user,
            RoleScopeEnum.Platform,
            null,
            "support_staff");
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginPlatformAsync(
            new PlatformLoginRequest("user@example.com", "Passw0rd!"));

        Assert.Equal(LoginFailure.NoActiveRole, result.Failure);
        VerifyTokenNeverCreated();
    }

    // ---------- LoginTenantAsync ----------

    [Fact]
    public async Task LoginTenantAsync_UserWithRoles_SucceedsAndPassesSortedRoleCodes()
    {
        var tenant = await SeedActiveTenantAsync();
        var user = NewUser(tenant.Id);
        ArrangeUser(user);
        await SeedRoleAsync(user, RoleScopeEnum.Tenant, tenant.Id, "b_role");
        await SeedRoleAsync(user, RoleScopeEnum.Tenant, tenant.Id, "a_role");
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginTenantAsync(
            new TenantLoginRequest("chung-cu-a", "user@example.com", "Passw0rd!"));

        Assert.True(result.IsSuccess);
        Assert.Equal(LoginFailure.None, result.Failure);
        Assert.NotNull(result.Value);
        VerifyTokenCreated(
            user,
            RoleScopeEnum.Tenant,
            "a_role",
            "b_role");
    }

    [Fact]
    public async Task LoginTenantAsync_RevokedRoleIsExcluded_OnlyActiveRoleIsUsed()
    {
        var tenant = await SeedActiveTenantAsync();
        var user = NewUser(tenant.Id);
        ArrangeUser(user);
        await SeedRoleAsync(user, RoleScopeEnum.Tenant, tenant.Id, "active_role");
        await SeedRoleAsync(
            user,
            RoleScopeEnum.Tenant,
            tenant.Id,
            "revoked_role",
            revoked: true);
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginTenantAsync(
            new TenantLoginRequest("chung-cu-a", "user@example.com", "Passw0rd!"));

        Assert.True(result.IsSuccess);
        VerifyTokenCreated(user, RoleScopeEnum.Tenant, "active_role");
    }

    [Fact]
    public async Task LoginTenantAsync_OnlyRoleIsDeleted_ReturnsNoActiveRole()
    {
        var tenant = await SeedActiveTenantAsync();
        var user = NewUser(tenant.Id);
        ArrangeUser(user);
        await SeedRoleAsync(
            user,
            RoleScopeEnum.Tenant,
            tenant.Id,
            "manager",
            roleDeleted: true);
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginTenantAsync(
            new TenantLoginRequest("chung-cu-a", "user@example.com", "Passw0rd!"));

        Assert.Equal(LoginFailure.NoActiveRole, result.Failure);
        VerifyTokenNeverCreated();
    }

    [Fact]
    public async Task LoginTenantAsync_RoleBelongsToAnotherTenant_ReturnsNoActiveRole()
    {
        var tenant = await SeedActiveTenantAsync();
        var user = NewUser(tenant.Id);
        ArrangeUser(user);
        await SeedRoleAsync(
            user,
            RoleScopeEnum.Tenant,
            Guid.NewGuid(),
            "manager");
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginTenantAsync(
            new TenantLoginRequest("chung-cu-a", "user@example.com", "Passw0rd!"));

        Assert.Equal(LoginFailure.NoActiveRole, result.Failure);
        VerifyTokenNeverCreated();
    }

    [Fact]
    public async Task LoginTenantAsync_RoleHasPlatformScope_ReturnsNoActiveRole()
    {
        var tenant = await SeedActiveTenantAsync();
        var user = NewUser(tenant.Id);
        ArrangeUser(user);
        await SeedRoleAsync(
            user,
            RoleScopeEnum.Platform,
            null,
            "platform_role",
            userRoleScope: RoleScopeEnum.Tenant,
            userRoleTenantId: tenant.Id);
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginTenantAsync(
            new TenantLoginRequest("chung-cu-a", "user@example.com", "Passw0rd!"));

        Assert.Equal(LoginFailure.NoActiveRole, result.Failure);
        VerifyTokenNeverCreated();
    }
}