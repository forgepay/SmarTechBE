using CryptoOnRamp.DAL.Models;

namespace CryptoOnRamp.DAL.Repositories.Interfaces;

public interface ITelegramUserRepository : IRepository<TelegramUserDb>
{
    Task<TelegramUserDb> AddAsync(long telegramId, int userId, CancellationToken cancellationToken);
    Task<TelegramUserDb?> RemoveAsync(long telegramId, CancellationToken cancellationToken);
    Task<TelegramUserDb?> GetByTelegramIdOrDefaultAsync(long telegramId, CancellationToken cancellationToken);
    Task<List<TelegramUserDb>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken);
}