using CryptoOnRamp.BLL.Models;

namespace CryptoOnRamp.BLL.Interfaces;

public interface IUserService
{
    Task<User> CreateAsync(CreateUserReuqest createUser);
    Task DeleteAsync(int userId, CancellationToken cancellationToken);
    int GetCurrentUserId();
    Task<IEnumerable<User>> GetUsersAsync(UserRole? role, int? parentId);

    Task ChangePasswordAsync(int userId, string oldPassword, string newPassword, bool checkOldPasswordVerification = true);
    Task CreateSelfRegisteredAsync(CreateSelfUserReuqest model);
    bool IsValidPassword(string text);

    Task UpdateUserWalletAsync(int userId, string newWalletAddress);

}
