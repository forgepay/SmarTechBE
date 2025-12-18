using CryptoOnRamp.BLL.Models;
using System.Security.Claims;

namespace CryptoOnRamp.BLL.Interfaces;

public interface IAuthenticationService
{
    Task<AuthenticationContext> GenerateAuthenticationContextAsync(string userId, IEnumerable<Claim> claims, TimeSpan accessTokenExpiry, TimeSpan refreshTokenExpiry, User userClientInfo, CancellationToken cancellationToken);
    Task<AuthenticationContext> RefreshAuthenticationContextAsync(string refreshToken, TimeSpan accessTokenExpiry, TimeSpan refreshTokenExpiry, User userClientInfo, CancellationToken cancellationToken);
}
