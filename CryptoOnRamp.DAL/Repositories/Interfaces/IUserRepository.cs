using CryptoOnRamp.DAL.Models;

namespace CryptoOnRamp.DAL.Repositories.Interfaces;

public interface IUserRepository : IRepository<UserDb>
{
    Task<UserDb?> GetUserOrDefaultAsync(int userId, CancellationToken cancellationToken);
    Task<UserDb?> GetActiveUserOrDefaultAsync(int userId, CancellationToken cancellationToken);
    Task<UserDb?> GetAdminOrDefaultAsync(CancellationToken cancellationToken);
    Task<UserDb?> GetUserByApiKeyHashOrDefaultAsync(string apiKeyHash, CancellationToken cancellationToken);
    Task<IEnumerable<UserDb>> GetAgentsBySuperAgentIdAsync(int superAgentId);
    Task<UserDb> UpdateUserAsync(UserDb user, CancellationToken cancellationToken);
}