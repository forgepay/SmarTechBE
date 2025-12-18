using CryptoOnRamp.DAL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;
using MicPic.Infrastructure.Exceptions;
using MicPic.Infrastructure.Extensions;

namespace CryptoOnRamp.BLL.Extensions;

public static class UserRepositoryExtensions
{
    public static async Task<UserDb> GetActiveUserAsync(this IUserRepository userRepository, int userId, CancellationToken cancellationToken) =>
        await userRepository.GetActiveUserOrDefaultAsync(userId, cancellationToken)
            ?? throw new AppException($"User not found.", BusinessErrorCodes.NotFound)
                .WithData("userId", userId);

    public static async Task<UserDb> GetAdminAsync(this IUserRepository userRepository, CancellationToken cancellationToken) =>
        await userRepository.GetAdminOrDefaultAsync(cancellationToken)
            ?? throw new AppException($"Admin user not found.", BusinessErrorCodes.NotFound);
}