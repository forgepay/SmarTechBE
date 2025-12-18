using System.Security.Cryptography;
using System.Text;
using CryptoOnRamp.BLL.Extensions;
using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.DAL.Repositories.Interfaces;

namespace CryptoOnRamp.BLL.Services;

internal class ApiKeyService(
    IUserRepository userRepository) : IApiKeyService
{
    private readonly IUserRepository _userRepository = userRepository;


    #region IApiKeyService Implementation

    public async Task<string> GenerateAsync(int userId, CancellationToken cancellationToken)
    {
        var token = GenerateRandomToken();

        var user = await _userRepository
            .GetActiveUserAsync(userId, cancellationToken);

        user.ApiKeyName = BuildKeyName(token);
        user.ApiKeyHash = CalculateHash(token);

        await _userRepository
            .UpdateUserAsync(user, cancellationToken);

        return token;
    }

    public async Task RemoveAsync(int userId, CancellationToken cancellationToken)
    {
        var token = GenerateRandomToken();

        var user = await _userRepository
            .GetActiveUserAsync(userId, cancellationToken);

        user.ApiKeyName = null;
        user.ApiKeyHash = null;

        await _userRepository
            .UpdateUserAsync(user, cancellationToken);
    }

    public async Task<string?> GetNameAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository
            .GetActiveUserAsync(userId, cancellationToken);

        return user.ApiKeyName;
    }

    public async Task<bool> ValidateAsync(int userId, string token, CancellationToken cancellationToken)
    {
        var user = await _userRepository
            .GetActiveUserAsync(userId, cancellationToken);

        if (string.IsNullOrWhiteSpace(user.ApiKeyHash))
            return false;

        var hash = CalculateHash(token);

        return hash == user.ApiKeyHash;
    }

    public async Task<int?> GetUserIdByApiKeyOrDefaultAsync(string apiKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return null;

        var user = await _userRepository
            .GetUserByApiKeyHashOrDefaultAsync(CalculateHash(apiKey), cancellationToken);

        return user?.Id;
    }

    #endregion


    #region Private Methods

    private static string BuildKeyName(string input)
    {
        return $"{input[..4]}*****";
    }

    private static string CalculateHash(string input)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));

        return Convert.ToBase64String(hashBytes);
    }

    private static string GenerateRandomToken(int length = 32)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        var random = new Random();
        var secretChars = new char[length];

        for (int i = 0; i < length; i++)
        {
            secretChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(secretChars);
    }

    #endregion
}
