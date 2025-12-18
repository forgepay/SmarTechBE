namespace CryptoOnRamp.BLL.Interfaces;

public interface IApiKeyService
{
    Task<string> GenerateAsync(int userId, CancellationToken cancellationToken);
    Task RemoveAsync(int userId, CancellationToken cancellationToken);
    Task<string?> GetNameAsync(int userId, CancellationToken cancellationToken);
    Task<bool> ValidateAsync(int userId, string token, CancellationToken cancellationToken);
    Task<int?> GetUserIdByApiKeyOrDefaultAsync(string apiKey, CancellationToken cancellationToken);
}
