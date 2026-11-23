using System.Security.Claims;
using System.Text.Json;
using EzCondo.BackEndServer.Contracts.SubscriptionPlans;
using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Infrastructure.Authentication;
using EzCondo.BackEndServer.Services.SubscriptionPlans;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EzCondo.BackEndServer.UnitTests.Services.SubscriptionPlans;

public class SubscriptionPlanServiceTests
{
    // Mỗi test (mỗi instance của class) dùng một database giả riêng.
    private readonly DbContextOptions<ApplicationDbContext> _options =
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private readonly Guid _adminId = Guid.NewGuid();

    private ApplicationDbContext NewDb() => new(_options);

    private SubscriptionPlanService NewService(
        ApplicationDbContext db,
        bool signedIn = true)
    {
        return new SubscriptionPlanService(
            db,
            CreateAccessor(signedIn ? _adminId : null));
    }

    private static IHttpContextAccessor CreateAccessor(Guid? userId)
    {
        var context = new DefaultHttpContext();

        if (userId.HasValue)
        {
            context.User = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[]
                    {
                        new Claim(AppClaimTypes.Subject, userId.Value.ToString())
                    },
                    "test"));
        }

        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(x => x.HttpContext).Returns(context);
        return accessor.Object;
    }

    private static JsonElement Json(string raw)
    {
        using var document = JsonDocument.Parse(raw);
        return document.RootElement.Clone();
    }

    private static CreateSubscriptionPlanRequest NewCreateRequest()
    {
        return new CreateSubscriptionPlanRequest
        {
            Name = "Goi Co Ban",
            BillingCycleMonths = 12,
            Price = 500000m
        };
    }

    private static UpdateSubscriptionPlanRequest NewUpdateRequest()
    {
        return new UpdateSubscriptionPlanRequest
        {
            Name = "Goi Moi",
            Price = 250000m,
            IsAvailable = false
        };
    }

    private async Task<SubscriptionPlan> SeedPlanAsync(
        string name = "Plan A",
        bool isAvailable = true,
        bool isDeleted = false)
    {
        await using var db = NewDb();

        var plan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "mo ta",
            BillingCycleMonths = 12,
            Price = 100000m,
            FeatureLimits = "{}",
            IsAvailable = isAvailable,
            IsDeleted = isDeleted,
            CreatedByAdmin = Guid.NewGuid()
        };

        db.SubscriptionPlans.Add(plan);
        await db.SaveChangesAsync();
        return plan;
    }

    private async Task SeedSubscriptionAsync(Guid planId, string legalName)
    {
        await using var db = NewDb();

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Code = "TEN-" + legalName,
            Slug = legalName.ToLowerInvariant(),
            LegalName = legalName,
            TaxCode = "TAX-" + legalName,
            Address = "12 Nguyen Hue, Da Nang"
        };

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            PlanId = planId,
            BillingCycleMonthsSnapshot = 12,
            Status = SubscriptionStatusEnum.Active,
            StartsAt = new DateOnly(2026, 1, 1),
            EndsAt = new DateOnly(2026, 12, 31)
        };

        db.Tenants.Add(tenant);
        db.Subscriptions.Add(subscription);
        await db.SaveChangesAsync();
    }

    private async Task<SubscriptionPlan> LoadPlanAsync(Guid id)
    {
        await using var db = NewDb();
        return await db.SubscriptionPlans
            .IgnoreQueryFilters()
            .SingleAsync(x => x.Id == id);
    }

    // ---------- CreateAsync ----------

    [Fact]
    public async Task CreateAsync_NullRequest_ThrowsArgumentNullException()
    {
        await using var db = NewDb();
        var service = NewService(db);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.CreateAsync(null!));
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_SavesTrimmedValuesAndCurrentAdmin()
    {
        await using var db = NewDb();
        var service = NewService(db);
        var request = NewCreateRequest();
        request.Name = "  Goi Co Ban  ";
        request.Description = "  Mo ta  ";

        var response = await service.CreateAsync(request);

        Assert.Equal("Goi Co Ban", response.Name);
        Assert.Equal("Mo ta", response.Description);
        Assert.Equal(12, response.BillingCycleMonths);
        Assert.Equal(500000m, response.Price);
        Assert.True(response.IsAvailable);
        Assert.NotEqual(default(DateTimeOffset), response.CreatedAt);

        await using var verify = NewDb();
        var saved = await verify.SubscriptionPlans
            .IgnoreQueryFilters()
            .SingleAsync();
        Assert.Equal(response.Id, saved.Id);
        Assert.Equal(_adminId, saved.CreatedByAdmin);
    }

    [Fact]
    public async Task CreateAsync_BlankDescription_IsStoredAsNull()
    {
        await using var db = NewDb();
        var service = NewService(db);
        var request = NewCreateRequest();
        request.Description = "   ";

        var response = await service.CreateAsync(request);

        Assert.Null(response.Description);
    }

    [Fact]
    public async Task CreateAsync_WithoutFeatureLimits_StoresEmptyJsonObject()
    {
        await using var db = NewDb();
        var service = NewService(db);

        var response = await service.CreateAsync(NewCreateRequest());

        Assert.Equal(JsonValueKind.Object, response.FeatureLimits.ValueKind);

        await using var verify = NewDb();
        var saved = await verify.SubscriptionPlans
            .IgnoreQueryFilters()
            .SingleAsync();
        Assert.Equal("{}", saved.FeatureLimits);
    }

    [Fact]
    public async Task CreateAsync_WithFeatureLimits_KeepsJsonContent()
    {
        await using var db = NewDb();
        var service = NewService(db);
        var request = NewCreateRequest();
        request.FeatureLimits = Json("{\"maxApartments\":100}");

        var response = await service.CreateAsync(request);

        Assert.Equal(
            100,
            response.FeatureLimits.GetProperty("maxApartments").GetInt32());
    }

    [Fact]
    public async Task CreateAsync_NoCurrentUser_ThrowsUnauthorizedAndSavesNothing()
    {
        await using var db = NewDb();
        var service = NewService(db, signedIn: false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.CreateAsync(NewCreateRequest()));

        await using var verify = NewDb();
        Assert.False(await verify.SubscriptionPlans.AnyAsync());
    }

    // ---------- UpdateAsync ----------

    [Fact]
    public async Task UpdateAsync_NullRequest_ThrowsArgumentNullException()
    {
        await using var db = NewDb();
        var service = NewService(db);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.UpdateAsync(Guid.NewGuid(), null!));
    }

    [Fact]
    public async Task UpdateAsync_PlanNotFound_ReturnsNull()
    {
        await using var db = NewDb();
        var service = NewService(db);

        var response = await service.UpdateAsync(
            Guid.NewGuid(),
            NewUpdateRequest());

        Assert.Null(response);
    }

    [Fact]
    public async Task UpdateAsync_PlanIsDeleted_ReturnsNull()
    {
        var plan = await SeedPlanAsync(isDeleted: true);
        await using var db = NewDb();
        var service = NewService(db);

        var response = await service.UpdateAsync(plan.Id, NewUpdateRequest());

        Assert.Null(response);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesEditableFieldsAndLeavesBillingCycleUnchanged()
    {
        var plan = await SeedPlanAsync();
        await using var db = NewDb();
        var service = NewService(db);
        var request = NewUpdateRequest();
        request.Name = "  Goi Moi  ";
        request.Description = "   ";
        request.FeatureLimits = Json("{\"maxApartments\":50}");

        var response = await service.UpdateAsync(plan.Id, request);

        Assert.NotNull(response);
        Assert.Equal("Goi Moi", response!.Name);
        Assert.Null(response.Description);
        Assert.Equal(250000m, response.Price);
        Assert.False(response.IsAvailable);
        Assert.Equal(12, response.BillingCycleMonths);
        Assert.Equal(
            50,
            response.FeatureLimits.GetProperty("maxApartments").GetInt32());
        Assert.NotNull(response.UpdatedAt);

        var saved = await LoadPlanAsync(plan.Id);
        Assert.Equal("Goi Moi", saved.Name);
        Assert.Equal(_adminId, saved.UpdatedByAdmin);
    }

    // ---------- GetAsync ----------

    [Fact]
    public async Task GetAsync_ReturnsOnlyNotDeletedPlansOrderedByName()
    {
        await SeedPlanAsync("C Plan");
        await SeedPlanAsync("A Plan");
        await SeedPlanAsync("B Plan", isDeleted: true);
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.GetAsync();

        Assert.Equal(
            new[] { "A Plan", "C Plan" },
            result.Select(x => x.Name).ToArray());
    }

    [Fact]
    public async Task GetAsync_NoPlans_ReturnsEmptyList()
    {
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.GetAsync();

        Assert.Empty(result);
    }

    // ---------- DeleteAsync ----------

    [Fact]
    public async Task DeleteAsync_PlanNotFound_ReturnsFalse()
    {
        await using var db = NewDb();
        var service = NewService(db);

        Assert.False(await service.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAsync_AlreadyDeletedPlan_ReturnsFalse()
    {
        var plan = await SeedPlanAsync(isDeleted: true);
        await using var db = NewDb();
        var service = NewService(db);

        Assert.False(await service.DeleteAsync(plan.Id));
    }

    [Fact]
    public async Task DeleteAsync_ExistingPlan_SoftDeletesAndMakesUnavailable()
    {
        var plan = await SeedPlanAsync(isAvailable: true);
        await using var db = NewDb();
        var service = NewService(db);

        var deleted = await service.DeleteAsync(plan.Id);

        Assert.True(deleted);
        var saved = await LoadPlanAsync(plan.Id);
        Assert.True(saved.IsDeleted);
        Assert.False(saved.IsAvailable);
        Assert.Equal(_adminId, saved.DeletedByAdmin);
        Assert.NotNull(saved.DeletedAt);
    }

    // ---------- DiscontinueAsync ----------

    [Fact]
    public async Task DiscontinueAsync_PlanNotFound_ReturnsNull()
    {
        await using var db = NewDb();
        var service = NewService(db);

        Assert.Null(await service.DiscontinueAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DiscontinueAsync_AvailablePlan_BecomesUnavailable()
    {
        var plan = await SeedPlanAsync(isAvailable: true);
        await using var db = NewDb();
        var service = NewService(db);

        var response = await service.DiscontinueAsync(plan.Id);

        Assert.NotNull(response);
        Assert.False(response!.IsAvailable);
        var saved = await LoadPlanAsync(plan.Id);
        Assert.False(saved.IsAvailable);
        Assert.Equal(_adminId, saved.UpdatedByAdmin);
    }

    [Fact]
    public async Task DiscontinueAsync_AlreadyUnavailablePlan_ReturnsPlanWithoutChangingIt()
    {
        var plan = await SeedPlanAsync(isAvailable: false);
        await using var db = NewDb();
        var service = NewService(db);

        var response = await service.DiscontinueAsync(plan.Id);

        Assert.NotNull(response);
        Assert.False(response!.IsAvailable);
        var saved = await LoadPlanAsync(plan.Id);
        Assert.Null(saved.UpdatedByAdmin);
    }

    // ---------- GetTenantsByPlanAsync ----------

    [Fact]
    public async Task GetTenantsByPlanAsync_NullRequest_ThrowsArgumentNullException()
    {
        await using var db = NewDb();
        var service = NewService(db);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.GetTenantsByPlanAsync(Guid.NewGuid(), null!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetTenantsByPlanAsync_PageNumberBelowOne_ThrowsArgumentOutOfRange(
        int pageNumber)
    {
        await using var db = NewDb();
        var service = NewService(db);
        var request = new GetSubscriptionPlanTenantsRequest
        {
            PageNumber = pageNumber,
            PageSize = 10
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.GetTenantsByPlanAsync(Guid.NewGuid(), request));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    [InlineData(-5)]
    public async Task GetTenantsByPlanAsync_PageSizeOutOfRange_ThrowsArgumentOutOfRange(
        int pageSize)
    {
        await using var db = NewDb();
        var service = NewService(db);
        var request = new GetSubscriptionPlanTenantsRequest
        {
            PageNumber = 1,
            PageSize = pageSize
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.GetTenantsByPlanAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task GetTenantsByPlanAsync_PlanNotFound_ReturnsPlanNotFound()
    {
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.GetTenantsByPlanAsync(
            Guid.NewGuid(),
            new GetSubscriptionPlanTenantsRequest());

        Assert.False(result.IsSuccess);
        Assert.Equal(
            GetSubscriptionPlanTenantsFailure.PlanNotFound,
            result.Failure);
    }

    [Fact]
    public async Task GetTenantsByPlanAsync_PlanWithoutSubscriptions_ReturnsEmptyPage()
    {
        var plan = await SeedPlanAsync("Plan Empty");
        await using var db = NewDb();
        var service = NewService(db);

        var result = await service.GetTenantsByPlanAsync(
            plan.Id,
            new GetSubscriptionPlanTenantsRequest());

        Assert.True(result.IsSuccess);
        Assert.Equal("Plan Empty", result.Value!.PlanName);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Equal(0, result.Value.TotalPages);
        Assert.Empty(result.Value.Items);
    }

    [Fact]
    public async Task GetTenantsByPlanAsync_ManySubscriptions_PagesOrderedByLegalName()
    {
        var plan = await SeedPlanAsync("Plan Main");
        var otherPlan = await SeedPlanAsync("Plan Other");
        await SeedSubscriptionAsync(plan.Id, "Charlie");
        await SeedSubscriptionAsync(plan.Id, "Alpha");
        await SeedSubscriptionAsync(plan.Id, "Bravo");
        await SeedSubscriptionAsync(otherPlan.Id, "Zulu");

        await using var db = NewDb();
        var service = NewService(db);

        var page1 = await service.GetTenantsByPlanAsync(
            plan.Id,
            new GetSubscriptionPlanTenantsRequest { PageNumber = 1, PageSize = 2 });
        var page2 = await service.GetTenantsByPlanAsync(
            plan.Id,
            new GetSubscriptionPlanTenantsRequest { PageNumber = 2, PageSize = 2 });
        var page3 = await service.GetTenantsByPlanAsync(
            plan.Id,
            new GetSubscriptionPlanTenantsRequest { PageNumber = 3, PageSize = 2 });

        Assert.True(page1.IsSuccess);
        Assert.Equal(3, page1.Value!.TotalCount);
        Assert.Equal(2, page1.Value.TotalPages);
        Assert.Equal(
            new[] { "Alpha", "Bravo" },
            page1.Value.Items.Select(x => x.LegalName).ToArray());
        Assert.Equal(
            new[] { "Charlie" },
            page2.Value!.Items.Select(x => x.LegalName).ToArray());
        Assert.Empty(page3.Value!.Items);
        Assert.Equal(2, page3.Value.TotalPages);
    }
}