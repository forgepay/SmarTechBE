using CryptoOnRamp.DAL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CryptoOnRamp.DAL.Repositories.Implementations;

public class FeeSchemeRepository
: EntityFrameworkRepository<FeeSchemeDb, ApplicationContext>, IFeeSchemeRepository
{
    public FeeSchemeRepository(ApplicationContext context) : base(context) { }

    public async Task<decimal?> GetPercentAsync(FeeType type, int? targetUserId)
    {
        var row = await DbSet.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Type == type && x.TargetUserId == targetUserId);
        return row?.Percent;
    }

    public async Task UpsertAsync(FeeType type, int? targetUserId, decimal percent, int actorUserId, DateTime now)
    {
        var existing = await DbSet.FirstOrDefaultAsync(x => x.Type == type && x.TargetUserId == targetUserId);
        if (existing is null)
        {
            var row = new FeeSchemeDb
            {
                Type = type,
                TargetUserId = targetUserId,
                Percent = percent,
                UpdatedByUserId = actorUserId,
                UpdatedAt = now
            };
            await DbSet.AddAsync(row);
        }
        else
        {
            existing.Percent = percent;
            existing.UpdatedByUserId = actorUserId;
            existing.UpdatedAt = now;
            Context.Entry(existing).State = EntityState.Modified;
        }
        await Context.SaveChangesAsync();
    }

    public async Task<List<(int SuperAgentId, decimal Percent)>> GetAllSuperAgentPercentsAsync()
    {
        var list = await DbSet.AsNoTracking()
 .Where(x => x.Type == FeeType.SuperAgent && x.TargetUserId != null)
            .Select(x => new { SuperAgentId = x.TargetUserId!.Value, x.Percent })
            .ToListAsync();

        return list.Select(x => (x.SuperAgentId, x.Percent)).ToList();
    }

    public async Task<List<(int AgentId, decimal Percent)>> GetAgentPercentsForSuperAgentAsync(int superAgentId)
    {
        var agentsQ = Context.Set<UserDb>()
            .Where(u => u.Role == UserRoleDb.Agent && u.CreatedById == superAgentId)
            .Select(u => u.Id);

        var list = await DbSet.AsNoTracking()
            .Where(x => x.Type == FeeType.Agent && x.TargetUserId != null && agentsQ.Contains(x.TargetUserId.Value))
            .Select(x => new { AgentId = x.TargetUserId!.Value, x.Percent })
            .ToListAsync();

        return list.Select(x => (x.AgentId, x.Percent)).ToList();
    }

    public async Task<List<(int AgentId, decimal Percent)>> GetAllAgentPercentsAsync()
    {
        var list = await DbSet.AsNoTracking()
            .Where(x => x.Type == FeeType.Agent && x.TargetUserId != null)
            .Select(x => new { AgentId = x.TargetUserId!.Value, x.Percent })
            .ToListAsync();

        return list.Select(x => (x.AgentId, x.Percent)).ToList();
    }
}
