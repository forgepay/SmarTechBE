using System.Security.Claims;

namespace MicPic.Infrastructure.Extensions;

public static class ClaimPrincipalExtensions
{
    public static int Id(this ClaimsPrincipal? user)
    {
        if (user is null || user.Identity is null || !user.Identity.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var idClaim = user.Claims
            .SingleOrDefault(c => string.Equals("X-SC-ID", c.Type, StringComparison.OrdinalIgnoreCase))?
            .Value;

        if (string.IsNullOrEmpty(idClaim))
            throw new UnauthorizedAccessException("User ID not found in claims.");

        if (!int.TryParse(idClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid user ID format in claims.");

        return userId;
    }
}