using CryptoOnRamp.BLL.Models;

namespace CryptoOnRamp.BLL.Interfaces;

public interface IIdentityService
{
    Task GeneratePasswordResetTokenAsync(string email);
    Task<AuthenticationContext> LoginAsync(string email, string passcode);
    Task<AuthenticationContext> RefreshTokenAsync(string email, string refreshToken, CancellationToken cancellationToken);
    Task ResetPasswordAsync(string email, string token, string newPassword);
}
