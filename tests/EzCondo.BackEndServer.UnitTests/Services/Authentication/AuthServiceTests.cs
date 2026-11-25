using EzCondo.BackEndServer.Contracts.Authentication.Platform;
using EzCondo.BackEndServer.Contracts.Authentication.Tenant;
using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Infrastructure.Authentication;
using EzCondo.BackEndServer.Services.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EzCondo.BackEndServer.UnitTests.Services.Authentication;

public class AuthServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 11, 25, 0, 0, 0, TimeSpan.Zero);

    private readonly DbContextOptions<ApplicationDbContext> _options =
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private readonly Mock<UserManager<User>> _userManager = CreateUserManager();
    private readonly Mock<SignInManager<User>> _signInManager;
    private readonly Mock<IJwtTokenService> _jwt = new();

    public AuthServiceTests()
    {
        _signInManager = CreateSignInManager(_userManager.Object);
    }

    // ---------- Dựng đối tượng ----------

    private static Mock<UserManager<User>> CreateUserManager()
    {
        var store = new Mock<IUserStore<User>>();

        return new Mock<UserManager<User>>(
            store.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static Mock<SignInManager<User>> CreateSignInManager(
        UserManager<User> userManager)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();

        return new Mock<SignInManager<User>>(
            userManager,
            accessor.Object,
            claimsFactory.Object,
            null!, null!, null!, null!);
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

    private static User NewUser(
        Guid? tenantId = null,
        UserStatusEnum status = UserStatusEnum.Active,
        bool emailConfirmed = true,
        bool isDeleted = false)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            UserName = "user@example.com",
            Email = "user@example.com",
            FullName = "Nguyen Van A",
            TenantId = tenantId,
            Status = status,
            EmailConfirmed = emailConfirmed,
            IsDeleted = isDeleted
        };
    }

    private async Task<Tenant> SeedTenantAsync(
        string slug,
        TenantStatusEnum status,
        DateTimeOffset? trialEndsAt = null)
    {
        await using var db = NewDb();

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Code = "TEN-" + slug,
            Slug = slug,
            LegalName = "Chung cu " + slug,
            TaxCode = "TAX-" + slug,
            Address = "12 Nguyen Hue, Da Nang",
            Status = status,
            TrialEndsAt = trialEndsAt
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();
        return tenant;
    }

    private static PlatformLoginRequest PlatformRequest(
        string email = "user@example.com",
        string password = "Passw0rd!")
    {
        return new PlatformLoginRequest(email, password);
    }

    private static TenantLoginRequest TenantRequest(
        string slug = "chung-cu-a",
        string email = "user@example.com",
        string password = "Passw0rd!")
    {
        return new TenantLoginRequest(slug, email, password);
    }

    private void ArrangeUser(User? user)
    {
        _userManager
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
    }

    private void ArrangePassword(Microsoft.AspNetCore.Identity.SignInResult result)
    {
        _signInManager
            .Setup(x => x.CheckPasswordSignInAsync(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
            .ReturnsAsync(result);
    }

    private void VerifyPasswordNeverChecked()
    {
        _signInManager.Verify(
            x => x.CheckPasswordSignInAsync(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<bool>()),
            Times.Never);
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
    public async Task LoginPlatformAsync_NullRequest_ThrowsArgumentNullException()
    {
        await using var db = NewDb();
        var service = NewService(db);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.LoginPlatformAsync(null!));
    }

    [Fact]
    public async Task LoginPlatformAsync_UserNotFound_ReturnsInvalidCredentials()
    {
        ArrangeUser(null);
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginPlatformAsync(PlatformRequest());

        Assert.False(result.IsSuccess);
        Assert.Equal(LoginFailure.InvalidCredentials, result.Failure);
        VerifyPasswordNeverChecked();
    }

    [Fact]
    public async Task LoginPlatformAsync_EmailWithSpaces_IsTrimmedBeforeLookup()
    {
        _userManager
            .Setup(x => x.FindByEmailAsync("user@example.com"))
            .ReturnsAsync((User?)null);
        await using var db = NewDb();
        var service = NewService(db);

        await service.LoginPlatformAsync(PlatformRequest("  user@example.com  "));

        _userManager.Verify(
            x => x.FindByEmailAsync("user@example.com"),
            Times.Once);
    }

    [Fact]
    public async Task LoginPlatformAsync_TenantUser_ReturnsAccountUnavailableWithoutCheckingPassword()
    {
        ArrangeUser(NewUser(tenantId: Guid.NewGuid()));
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginPlatformAsync(PlatformRequest());

        Assert.Equal(LoginFailure.AccountUnavailable, result.Failure);
        VerifyPasswordNeverChecked();
    }

    [Theory]
    [InlineData(true, UserStatusEnum.Active, true)]
    [InlineData(false, UserStatusEnum.Inviting, true)]
    [InlineData(false, UserStatusEnum.Active, false)]
    public async Task LoginPlatformAsync_UserCannotSignIn_ReturnsAccountUnavailable(
        bool isDeleted,
        UserStatusEnum status,
        bool emailConfirmed)
    {
        ArrangeUser(NewUser(
            status: status,
            emailConfirmed: emailConfirmed,
            isDeleted: isDeleted));
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginPlatformAsync(PlatformRequest());

        Assert.Equal(LoginFailure.AccountUnavailable, result.Failure);
        VerifyPasswordNeverChecked();
    }

    [Fact]
    public async Task LoginPlatformAsync_LockedOut_ReturnsLockedOut()
    {
        ArrangeUser(NewUser());
        ArrangePassword(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginPlatformAsync(PlatformRequest());

        Assert.Equal(LoginFailure.LockedOut, result.Failure);
        VerifyTokenNeverCreated();
    }

    [Fact]
    public async Task LoginPlatformAsync_WrongPassword_ReturnsInvalidCredentialsAndCountsFailure()
    {
        var user = NewUser();
        ArrangeUser(user);
        ArrangePassword(Microsoft.AspNetCore.Identity.SignInResult.Failed);
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginPlatformAsync(
            PlatformRequest(password: "sai-mat-khau"));

        Assert.Equal(LoginFailure.InvalidCredentials, result.Failure);
        _signInManager.Verify(
            x => x.CheckPasswordSignInAsync(user, "sai-mat-khau", true),
            Times.Once);
        VerifyTokenNeverCreated();
    }

    [Fact]
    public async Task LoginPlatformAsync_CorrectPasswordButNoActiveRole_ReturnsNoActiveRole()
    {
        ArrangeUser(NewUser());
        ArrangePassword(Microsoft.AspNetCore.Identity.SignInResult.Success);
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginPlatformAsync(PlatformRequest());

        Assert.Equal(LoginFailure.NoActiveRole, result.Failure);
        VerifyTokenNeverCreated();
    }

    // ---------- LoginTenantAsync ----------

    [Fact]
    public async Task LoginTenantAsync_NullRequest_ThrowsArgumentNullException()
    {
        await using var db = NewDb();
        var service = NewService(db);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.LoginTenantAsync(null!));
    }

    [Fact]
    public async Task LoginTenantAsync_TenantSlugNotFound_ReturnsTenantUnavailableWithoutLookingUpUser()
    {
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginTenantAsync(TenantRequest("khong-ton-tai"));

        Assert.Equal(LoginFailure.TenantUnavailable, result.Failure);
        _userManager.Verify(
            x => x.FindByEmailAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Theory]
    [InlineData(TenantStatusEnum.Suspended)]
    [InlineData(TenantStatusEnum.Closed)]
    public async Task LoginTenantAsync_TenantSuspendedOrClosed_ReturnsTenantUnavailable(
        TenantStatusEnum status)
    {
        await SeedTenantAsync("chung-cu-a", status);
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginTenantAsync(TenantRequest());

        Assert.Equal(LoginFailure.TenantUnavailable, result.Failure);
    }

    [Fact]
    public async Task LoginTenantAsync_TrialExpired_ReturnsTenantUnavailable()
    {
        await SeedTenantAsync(
            "chung-cu-a",
            TenantStatusEnum.Trial,
            trialEndsAt: Now.AddDays(-1));
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginTenantAsync(TenantRequest());

        Assert.Equal(LoginFailure.TenantUnavailable, result.Failure);
    }

    [Fact]
    public async Task LoginTenantAsync_TrialWithoutEndDate_ReturnsTenantUnavailable()
    {
        await SeedTenantAsync(
            "chung-cu-a",
            TenantStatusEnum.Trial,
            trialEndsAt: null);
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginTenantAsync(TenantRequest());

        Assert.Equal(LoginFailure.TenantUnavailable, result.Failure);
    }

    [Fact]
    public async Task LoginTenantAsync_ActiveTenantWithSlugSpaces_IsFoundAndUserLookupFails()
    {
        await SeedTenantAsync("chung-cu-a", TenantStatusEnum.Active);
        ArrangeUser(null);
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginTenantAsync(
            TenantRequest(slug: "  chung-cu-a  "));

        Assert.Equal(LoginFailure.InvalidCredentials, result.Failure);
    }

    [Fact]
    public async Task LoginTenantAsync_TrialNotExpired_TenantIsAvailable()
    {
        await SeedTenantAsync(
            "chung-cu-a",
            TenantStatusEnum.Trial,
            trialEndsAt: Now.AddDays(5));
        ArrangeUser(null);
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginTenantAsync(TenantRequest());

        // Qua được bước kiểm tra tenant, dừng ở bước tìm user.
        Assert.Equal(LoginFailure.InvalidCredentials, result.Failure);
    }

    [Fact]
    public async Task LoginTenantAsync_UserBelongsToAnotherTenant_ReturnsAccountUnavailable()
    {
        await SeedTenantAsync("chung-cu-a", TenantStatusEnum.Active);
        ArrangeUser(NewUser(tenantId: Guid.NewGuid()));
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginTenantAsync(TenantRequest());

        Assert.Equal(LoginFailure.AccountUnavailable, result.Failure);
        VerifyPasswordNeverChecked();
    }

    [Theory]
    [InlineData(true, UserStatusEnum.Active, true)]
    [InlineData(false, UserStatusEnum.Inviting, true)]
    [InlineData(false, UserStatusEnum.Active, false)]
    public async Task LoginTenantAsync_UserCannotSignIn_ReturnsAccountUnavailable(
        bool isDeleted,
        UserStatusEnum status,
        bool emailConfirmed)
    {
        var tenant = await SeedTenantAsync("chung-cu-a", TenantStatusEnum.Active);
        ArrangeUser(NewUser(
            tenantId: tenant.Id,
            status: status,
            emailConfirmed: emailConfirmed,
            isDeleted: isDeleted));
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginTenantAsync(TenantRequest());

        Assert.Equal(LoginFailure.AccountUnavailable, result.Failure);
        VerifyPasswordNeverChecked();
    }

    [Fact]
    public async Task LoginTenantAsync_LockedOut_ReturnsLockedOut()
    {
        var tenant = await SeedTenantAsync("chung-cu-a", TenantStatusEnum.Active);
        ArrangeUser(NewUser(tenantId: tenant.Id));
        ArrangePassword(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginTenantAsync(TenantRequest());

        Assert.Equal(LoginFailure.LockedOut, result.Failure);
        VerifyTokenNeverCreated();
    }

    [Fact]
    public async Task LoginTenantAsync_WrongPassword_ReturnsInvalidCredentials()
    {
        var tenant = await SeedTenantAsync("chung-cu-a", TenantStatusEnum.Active);
        var user = NewUser(tenantId: tenant.Id);
        ArrangeUser(user);
        ArrangePassword(Microsoft.AspNetCore.Identity.SignInResult.Failed);
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginTenantAsync(
            TenantRequest(password: "sai-mat-khau"));

        Assert.Equal(LoginFailure.InvalidCredentials, result.Failure);
        _signInManager.Verify(
            x => x.CheckPasswordSignInAsync(user, "sai-mat-khau", true),
            Times.Once);
        VerifyTokenNeverCreated();
    }

    [Fact]
    public async Task LoginTenantAsync_CorrectPasswordButNoActiveRole_ReturnsNoActiveRole()
    {
        var tenant = await SeedTenantAsync("chung-cu-a", TenantStatusEnum.Active);
        ArrangeUser(NewUser(tenantId: tenant.Id));
        ArrangePassword(Microsoft.AspNetCore.Identity.SignInResult.Success);
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LoginTenantAsync(TenantRequest());

        Assert.Equal(LoginFailure.NoActiveRole, result.Failure);
        VerifyTokenNeverCreated();
    }

    // ---------- LogoutAllAsync ----------

    [Fact]
    public async Task LogoutAllAsync_UserNotFound_ReturnsUserNotFound()
    {
        var userId = Guid.NewGuid();
        _userManager
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((User?)null);
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LogoutAllAsync(userId);

        Assert.False(result.IsSuccess);
        Assert.Equal(LogoutFailure.UserNotFound, result.Failure);
    }

    [Fact]
    public async Task LogoutAllAsync_DeletedUser_ReturnsUserNotFoundAndDoesNotUpdateStamp()
    {
        var user = NewUser(isDeleted: true);
        _userManager
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LogoutAllAsync(user.Id);

        Assert.Equal(LogoutFailure.UserNotFound, result.Failure);
        _userManager.Verify(
            x => x.UpdateSecurityStampAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task LogoutAllAsync_UpdateSecurityStampFails_ReturnsUpdateSecurityStampFailed()
    {
        var user = NewUser();
        _userManager
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        _userManager
            .Setup(x => x.UpdateSecurityStampAsync(user))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Code = "Error", Description = "Loi" }));
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LogoutAllAsync(user.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(LogoutFailure.UpdateSecurityStampFailed, result.Failure);
    }

    [Fact]
    public async Task LogoutAllAsync_ValidUser_UpdatesSecurityStampAndSucceeds()
    {
        var user = NewUser();
        _userManager
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        _userManager
            .Setup(x => x.UpdateSecurityStampAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.LogoutAllAsync(user.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(LogoutFailure.None, result.Failure);
        _userManager.Verify(
            x => x.UpdateSecurityStampAsync(user),
            Times.Once);
    }
}

public class LoginResultTests
{
    [Fact]
    public void PlatformLoginResult_Failed_WithNone_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => PlatformLoginResult.Failed(LoginFailure.None));
    }

    [Fact]
    public void PlatformLoginResult_Success_WithNullValue_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => PlatformLoginResult.Success(null!));
    }

    [Theory]
    [InlineData(LoginFailure.InvalidCredentials)]
    [InlineData(LoginFailure.LockedOut)]
    [InlineData(LoginFailure.AccountUnavailable)]
    [InlineData(LoginFailure.TenantUnavailable)]
    [InlineData(LoginFailure.NoActiveRole)]
    public void PlatformLoginResult_Failed_WithReason_IsNotSuccessAndKeepsReason(
        LoginFailure failure)
    {
        var result = PlatformLoginResult.Failed(failure);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(failure, result.Failure);
    }

    [Fact]
    public void TenantLoginResult_Failed_WithNone_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => TenantLoginResult.Failed(LoginFailure.None));
    }

    [Fact]
    public void TenantLoginResult_Success_WithNullValue_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => TenantLoginResult.Success(null!));
    }

    [Theory]
    [InlineData(LoginFailure.InvalidCredentials)]
    [InlineData(LoginFailure.LockedOut)]
    [InlineData(LoginFailure.AccountUnavailable)]
    [InlineData(LoginFailure.TenantUnavailable)]
    [InlineData(LoginFailure.NoActiveRole)]
    public void TenantLoginResult_Failed_WithReason_IsNotSuccessAndKeepsReason(
        LoginFailure failure)
    {
        var result = TenantLoginResult.Failed(failure);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(failure, result.Failure);
    }

    [Fact]
    public void LogoutResult_Success_IsSuccess()
    {
        var result = LogoutResult.Success();

        Assert.True(result.IsSuccess);
        Assert.Equal(LogoutFailure.None, result.Failure);
    }

    [Fact]
    public void LogoutResult_Failed_WithNone_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => LogoutResult.Failed(LogoutFailure.None));
    }

    [Theory]
    [InlineData(LogoutFailure.UserNotFound)]
    [InlineData(LogoutFailure.UpdateSecurityStampFailed)]
    public void LogoutResult_Failed_WithReason_IsNotSuccessAndKeepsReason(
        LogoutFailure failure)
    {
        var result = LogoutResult.Failed(failure);

        Assert.False(result.IsSuccess);
        Assert.Equal(failure, result.Failure);
    }
}