using EzCondo.BackEndServer.Services.Tenants;

namespace EzCondo.BackEndServer.UnitTests.Services.Tenants;

public class TenantIdentityGeneratorTests
{
    private static readonly Guid FixedId =
        new("11111111-2222-3333-4444-555555555555");

    private const string FixedIdN = "11111111222233334444555555555555";

    private readonly TenantIdentityGenerator _generator = new();

    // TC-TIG-001
    [Fact]
    public void Generate_ValidInput_ReturnsExpectedCodeAndSlug()
    {
        var result = _generator.Generate(FixedId, "Chung cư An Bình");

        Assert.StartsWith("TEN-", result.Code);
        Assert.Equal(26, result.Code.Length);
        Assert.StartsWith("chung-cu-an-binh-", result.Slug);
        Assert.EndsWith(FixedIdN, result.Slug);
    }

    // TC-TIG-002
    [Fact]
    public void Generate_UppercaseAndExtraSpaces_NormalizesToSingleHyphens()
    {
        var result = _generator.Generate(FixedId, "  CHUNG CƯ   AN BÌNH  ");

        Assert.Equal($"chung-cu-an-binh-{FixedIdN}", result.Slug);
        Assert.DoesNotContain("--", result.Slug);
    }

    // TC-TIG-003
    [Fact]
    public void Generate_VietnameseDiacriticsAndD_RemovesAccents()
    {
        var result = _generator.Generate(FixedId, "Đại Đô Thị Mỹ Đình");

        Assert.StartsWith("dai-do-thi-my-dinh-", result.Slug);
    }

    // TC-TIG-004
    [Fact]
    public void Generate_SpecialCharacters_RemovesThem()
    {
        var result = _generator.Generate(FixedId, "Sunrise @ Central!!!");

        Assert.Equal($"sunrise-central-{FixedIdN}", result.Slug);
        Assert.DoesNotContain("@", result.Slug);
        Assert.DoesNotContain("!", result.Slug);
    }

    // TC-TIG-005
    [Fact]
    public void Generate_OnlySymbols_FallsBackToTenant()
    {
        var result = _generator.Generate(FixedId, "@@@ ###");

        Assert.Equal($"tenant-{FixedIdN}", result.Slug);
    }

    // TC-TIG-006
    [Fact]
    public void Generate_EmptyGuid_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => _generator.Generate(Guid.Empty, "Chung cư An Bình"));
    }

    // TC-TIG-007
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Generate_NullEmptyOrWhitespaceName_ThrowsArgumentException(
        string? legalName)
    {
        Assert.ThrowsAny<ArgumentException>(
            () => _generator.Generate(FixedId, legalName!));
    }
}
