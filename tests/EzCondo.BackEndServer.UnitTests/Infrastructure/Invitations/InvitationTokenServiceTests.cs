using System.Text.RegularExpressions;
using EzCondo.BackEndServer.Infrastructure.Invitations;

namespace EzCondo.BackEndServer.UnitTests.Infrastructure.Invitations;

public class InvitationTokenServiceTests
{
    private readonly InvitationTokenService _service = new();

    [Fact]
    public void Generate_ReturnsNonEmptyTokenAndHash()
    {
        var result = _service.Generate();

        Assert.False(string.IsNullOrWhiteSpace(result.Value));
        Assert.False(string.IsNullOrWhiteSpace(result.Hash));
    }

    [Fact]
    public void Generate_Token_IsBase64UrlOf32Bytes()
    {
        var result = _service.Generate();

        Assert.Equal(43, result.Value.Length);
        Assert.Matches("^[A-Za-z0-9_-]+$", result.Value);
    }

    [Fact]
    public void Generate_Hash_IsComputeHashOfToken()
    {
        var result = _service.Generate();

        Assert.Equal(_service.ComputeHash(result.Value), result.Hash);
    }

    [Fact]
    public void Generate_Hash_Is64UppercaseHexCharacters()
    {
        var result = _service.Generate();

        Assert.Equal(64, result.Hash.Length);
        Assert.True(Regex.IsMatch(result.Hash, "^[0-9A-F]{64}$"));
    }

    [Fact]
    public void Generate_CalledManyTimes_ProducesUniqueTokens()
    {
        var tokens = Enumerable.Range(0, 100)
            .Select(_ => _service.Generate().Value)
            .ToHashSet();

        Assert.Equal(100, tokens.Count);
    }

    [Fact]
    public void ComputeHash_KnownInput_ReturnsKnownSha256()
    {
        var hash = _service.ComputeHash("abc");

        Assert.Equal(
            "BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD",
            hash);
    }

    [Fact]
    public void ComputeHash_SameInput_ReturnsSameHash()
    {
        Assert.Equal(
            _service.ComputeHash("token-1"),
            _service.ComputeHash("token-1"));
    }

    [Fact]
    public void ComputeHash_DifferentInput_ReturnsDifferentHash()
    {
        Assert.NotEqual(
            _service.ComputeHash("token-1"),
            _service.ComputeHash("token-2"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ComputeHash_NullEmptyOrWhitespace_ThrowsArgumentException(
        string? token)
    {
        Assert.ThrowsAny<ArgumentException>(
            () => _service.ComputeHash(token!));
    }
}
