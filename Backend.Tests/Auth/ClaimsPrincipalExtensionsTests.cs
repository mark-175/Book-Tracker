using System.Security.Claims;
using BookTracker.Api.Auth;

namespace BookTracker.Api.Tests.Auth;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetUserId_ValidNameIdentifierClaim_ReturnsGuid()
    {
        var userId = Guid.NewGuid();
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var result = principal.GetUserId();

        Assert.Equal(userId, result);
    }

    [Fact]
    public void GetUserId_NoNameIdentifierClaim_ThrowsInvalidOperationException()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var ex = Assert.Throws<InvalidOperationException>(() => principal.GetUserId());
        Assert.Equal("No user ID claim found.", ex.Message);
    }

    [Fact]
    public void GetUserId_NameIdentifierClaimIsNotAGuid_ThrowsFormatException()
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "not-a-guid") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        Assert.Throws<FormatException>(() => principal.GetUserId());
    }
}
