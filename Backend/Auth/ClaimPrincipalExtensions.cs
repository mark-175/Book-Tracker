using System.Security.Claims;

namespace BookTracker.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new InvalidOperationException("No user ID claim found.");

        return Guid.Parse(idClaim);
    }
}