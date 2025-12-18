using CryptoOnRamp.BLL.Models;

namespace CryptoOnRamp.BLL.Interfaces;

public interface ITelegramUserService
{
    Task<TelegramUser> AddAsync(long telegramId, int userId, CancellationToken cancellationToken);
    Task<TelegramUser?> RemoveAsync(long telegramId, int userId, CancellationToken cancellationToken);
    Task<TelegramUser?> GetByTelegramIdOrDefaultAsync(long telegramId, CancellationToken cancellationToken);
    Task<List<TelegramUser>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken);
}
