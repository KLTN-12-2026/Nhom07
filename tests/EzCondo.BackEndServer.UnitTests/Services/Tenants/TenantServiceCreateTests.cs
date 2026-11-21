using EzCondo.BackEndServer.Contracts.Tenants;
using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Services.Tenants;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EzCondo.BackEndServer.UnitTests.Services.Tenants;

public class TenantServiceCreateTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    // DbInitializer và IHttpContextAccessor không được dùng ở các nhánh lỗi
    // nên truyền null.
    private static TenantService CreateService(
        ApplicationDbContext db,
        Mock<ITenantIdentityGenerator> identity)
    {
        return new TenantService(
            db,
            null!,
            identity.Object,
            TimeProvider.System,
            null!);
    }

    private static SubscriptionPlan NewPlan(
        bool isAvailable = true,
        bool isDeleted = false)
    {
        return new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = "Gói thử nghiệm",
            BillingCycleMonths = 1,
            Price = 100000m,
            FeatureLimits = "{}",
            IsAvailable = isAvailable,
            IsDeleted = isDeleted,
            CreatedByAdmin = Guid.NewGuid()
        };
    }

    private static Tenant NewTenant(string taxCode)
    {
        return new Tenant
        {
            Id = Guid.NewGuid(),
            Code = "TEN-EXISTING",
            Slug = "existing",
            LegalName = "Chung cư có sẵn",
            TaxCode = taxCode,
            Address = "12 Nguyễn Huệ, Đà Nẵng"
        };
    }

    private static CreateTenantRequest NewRequest(
        Guid planId,
        string taxCode = "0123456789")
    {
        return new CreateTenantRequest
        {
            LegalName = "Chung cư An Bình",
            TaxCode = taxCode,
            Address = "12 Nguyễn Huệ, Đà Nẵng",
            SubscriptionPlanId = planId
        };
    }

    [Fact]
    public async Task CreateAsync_NullRequest_ThrowsArgumentNullException()
    {
        await using var db = CreateDb();
        var service = CreateService(db, new Mock<ITenantIdentityGenerator>());

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.CreateAsync(null!));
    }

    [Fact]
    public async Task CreateAsync_PlanNotFound_ReturnsPlanNotFoundAndDoesNotGenerateIdentity()
    {
        await using var db = CreateDb();
        var identity = new Mock<ITenantIdentityGenerator>();
        var service = CreateService(db, identity);

        var result = await service.CreateAsync(NewRequest(Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            CreateTenantFailure.SubscriptionPlanNotFound,
            result.Failure);
        identity.Verify(
            x => x.Generate(It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_PlanIsDeleted_ReturnsPlanNotFound()
    {
        await using var db = CreateDb();
        var plan = NewPlan(isAvailable: true, isDeleted: true);
        db.SubscriptionPlans.Add(plan);
        await db.SaveChangesAsync();
        var service = CreateService(db, new Mock<ITenantIdentityGenerator>());

        var result = await service.CreateAsync(NewRequest(plan.Id));

        Assert.Equal(
            CreateTenantFailure.SubscriptionPlanNotFound,
            result.Failure);
    }

    [Fact]
    public async Task CreateAsync_PlanUnavailable_ReturnsPlanUnavailable()
    {
        await using var db = CreateDb();
        var plan = NewPlan(isAvailable: false);
        db.SubscriptionPlans.Add(plan);
        await db.SaveChangesAsync();
        var service = CreateService(db, new Mock<ITenantIdentityGenerator>());

        var result = await service.CreateAsync(NewRequest(plan.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            CreateTenantFailure.SubscriptionPlanUnavailable,
            result.Failure);
    }

    [Fact]
    public async Task CreateAsync_TaxCodeAlreadyExists_ReturnsTaxCodeAlreadyExists()
    {
        await using var db = CreateDb();
        var plan = NewPlan();
        db.SubscriptionPlans.Add(plan);
        db.Tenants.Add(NewTenant("0123456789"));
        await db.SaveChangesAsync();
        var identity = new Mock<ITenantIdentityGenerator>();
        var service = CreateService(db, identity);

        var result = await service.CreateAsync(NewRequest(plan.Id, "0123456789"));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            CreateTenantFailure.TaxCodeAlreadyExists,
            result.Failure);
        identity.Verify(
            x => x.Generate(It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_TaxCodeWithSurroundingSpaces_IsTrimmedBeforeDuplicateCheck()
    {
        await using var db = CreateDb();
        var plan = NewPlan();
        db.SubscriptionPlans.Add(plan);
        db.Tenants.Add(NewTenant("0123456789"));
        await db.SaveChangesAsync();
        var service = CreateService(db, new Mock<ITenantIdentityGenerator>());

        var result = await service.CreateAsync(NewRequest(plan.Id, "  0123456789  "));

        Assert.Equal(
            CreateTenantFailure.TaxCodeAlreadyExists,
            result.Failure);
    }
}
