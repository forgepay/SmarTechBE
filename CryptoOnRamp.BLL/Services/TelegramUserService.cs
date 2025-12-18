using AutoMapper;
using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;

namespace CryptoOnRamp.BLL.Services;

internal class TelegramUserService(ITelegramUserRepository telegramUserRepository, IMapper mapper) : ITelegramUserService
{
    private readonly ITelegramUserRepository _telegramUserRepository = telegramUserRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<TelegramUser> AddAsync(long telegramId, int userId, CancellationToken cancellationToken)
    {
        var dbTelegramUser = await _telegramUserRepository.GetByTelegramIdOrDefaultAsync(telegramId, cancellationToken);

        if (dbTelegramUser is not null)
        {
            if (dbTelegramUser.UserId != userId)
            {
                throw new InvalidOperationException("Telegram ID is already associated with another user.");
            }

            return _mapper.Map<TelegramUser>(dbTelegramUser);
        }

        dbTelegramUser = await _telegramUserRepository.AddAsync(telegramId, userId, cancellationToken);

        return _mapper.Map<TelegramUser>(dbTelegramUser);
    }

    public async Task<TelegramUser?> RemoveAsync(long telegramId, int userId, CancellationToken cancellationToken)
    {
        var dbTelegramUser = await _telegramUserRepository.GetByTelegramIdOrDefaultAsync(telegramId, cancellationToken);

        if (dbTelegramUser is null)
        {
            return null;
        }

        if (dbTelegramUser.UserId != userId)
        {
            throw new InvalidOperationException("Telegram ID is associated with another user.");
        }

        await _telegramUserRepository.RemoveAsync(telegramId, cancellationToken);

        return _mapper.Map<TelegramUser>(dbTelegramUser);
    }

    public async Task<List<TelegramUser>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        var dbTelegramUsers = await _telegramUserRepository.GetAllByUserIdAsync(userId, cancellationToken);

        return _mapper.Map<List<TelegramUser>>(dbTelegramUsers);
    }

    public async Task<TelegramUser?> GetByTelegramIdOrDefaultAsync(long telegramId, CancellationToken cancellationToken)
    {
        var dbTelegramUser = await _telegramUserRepository.GetByTelegramIdOrDefaultAsync(telegramId, cancellationToken);

        if (dbTelegramUser == null)
        {
            return null;
        }

        return _mapper.Map<TelegramUser>(dbTelegramUser);
    }
}
