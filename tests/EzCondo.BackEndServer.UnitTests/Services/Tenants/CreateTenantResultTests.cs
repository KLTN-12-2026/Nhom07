using EzCondo.BackEndServer.Services.Tenants;

namespace EzCondo.BackEndServer.UnitTests.Services.Tenants;

public class CreateTenantResultTests
{
    [Fact]
    public void Failed_WithNoneFailure_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => CreateTenantResult.Failed(CreateTenantFailure.None));
    }

    [Theory]
    [InlineData(CreateTenantFailure.SubscriptionPlanNotFound)]
    [InlineData(CreateTenantFailure.SubscriptionPlanUnavailable)]
    [InlineData(CreateTenantFailure.TaxCodeAlreadyExists)]
    [InlineData(CreateTenantFailure.GeneratedidentityCollision)]
    public void Failed_WithReason_IsNotSuccessAndKeepsReason(
        CreateTenantFailure failure)
    {
        var result = CreateTenantResult.Failed(failure);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(failure, result.Failure);
    }

    [Fact]
    public void Success_WithNullValue_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => CreateTenantResult.Success(null!));
    }
}
