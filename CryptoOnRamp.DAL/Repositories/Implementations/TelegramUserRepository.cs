using CryptoOnRamp.DAL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CryptoOnRamp.DAL.Repositories.Implementations;

internal class TelegramUserRepository : EntityFrameworkRepository<TelegramUserDb, ApplicationContext>, ITelegramUserRepository
{
    public TelegramUserRepository(ApplicationContext context)
        : base(context)
    {
    }

    public async Task<TelegramUserDb> AddAsync(long telegramId, int userId, CancellationToken cancellationToken)
    {
        var entity = new TelegramUserDb
        {
            TelegramId = telegramId,
            UserId = userId
        };

        await DbSet.AddAsync(entity, cancellationToken);

        await Context.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public async Task<TelegramUserDb?> RemoveAsync(long telegramId, CancellationToken cancellationToken)
    {
        var entity = await DbSet
            .FirstOrDefaultAsync(u => u.TelegramId == telegramId, cancellationToken);

        if (entity == null)
            return null;

        DbSet.Remove(entity);

        await Context.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public Task<List<TelegramUserDb>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        return DbSet
            .AsNoTracking()
            .Where(u => u.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public Task<TelegramUserDb?> GetByTelegramIdOrDefaultAsync(long telegramId, CancellationToken cancellationToken)
    {
        return DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.TelegramId == telegramId, cancellationToken);
    }
}