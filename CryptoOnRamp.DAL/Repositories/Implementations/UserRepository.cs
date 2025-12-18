using CryptoOnRamp.DAL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CryptoOnRamp.DAL.Repositories.Implementations;

internal class UserRepository : EntityFrameworkRepository<UserDb, ApplicationContext>, IUserRepository
{
    public UserRepository(ApplicationContext context)
        : base(context)
    {
    }

    public async Task<UserDb?> GetUserOrDefaultAsync(int userId, CancellationToken cancellationToken)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<UserDb?> GetActiveUserOrDefaultAsync(int userId, CancellationToken cancellationToken)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<UserDb?> GetAdminOrDefaultAsync(CancellationToken cancellationToken)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderBy(u => u.Id)
            .FirstOrDefaultAsync(u => u.Role == UserRoleDb.Admin, cancellationToken);
    }

    public async Task<UserDb?> GetUserByApiKeyHashOrDefaultAsync(string apiKeyHash, CancellationToken cancellationToken)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .FirstOrDefaultAsync(u => u.ApiKeyHash == apiKeyHash, cancellationToken);
    }

    public async Task<IEnumerable<UserDb>> GetAgentsBySuperAgentIdAsync(int superAgentId)
    {
        return await DbSet
            .Where(x => x.DeletedAt == null)
            .Where(u => u.CreatedById == superAgentId && u.Role == UserRoleDb.Agent)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<UserDb> UpdateUserAsync(UserDb user, CancellationToken cancellationToken)
    {
        Context.Entry(user).State = EntityState.Modified;

        await Context.SaveChangesAsync(cancellationToken);

        Context.Entry(user).State = EntityState.Deleted;

        return user;
    }
}