using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using CryptoOnRamp.DAL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;

namespace CryptoOnRamp.BLL.Services;

public class FeeService(IFeeSchemeRepository repo, IUserRepository users, IUserService current) : IFeeService
{
    private readonly IFeeSchemeRepository _repo = repo;
    private readonly IUserRepository _users = users;
    private readonly IUserService _current = current;

    public async Task<GetFeeSettingsResponse> GetSettingsAsync()
    {
        var me = await _users.GetFirstOrDefaultAsync(u => u.Id == _current.GetCurrentUserId());

        if (me?.Role == UserRoleDb.Admin)
        {
            var superList = (await _repo.GetAllSuperAgentPercentsAsync()).Select(x => new UserFeeInfo(x.SuperAgentId, x.Percent)).ToList();
            var agentList = (await _repo.GetAllAgentPercentsAsync()).Select(x => new UserFeeInfo(x.AgentId, x.Percent)).ToList();
            return new(null, superList, agentList);
        }

        if (me?.Role == UserRoleDb.SuperAgent)
        {
            var mySuper = await _repo.GetPercentAsync(FeeType.SuperAgent, me.Id);
            var agentList = (await _repo.GetAgentPercentsForSuperAgentAsync(me.Id)).Select(x => new UserFeeInfo(x.AgentId, x.Percent)).ToList();
            return new(mySuper, new(), agentList);
        }

        throw new UnauthorizedAccessException();
    }

    public async Task<(decimal sa, decimal ag)> ResolvePercentsForAgentAsync(int? agentUserId)
    {
        if (agentUserId is null)
            throw new ArgumentNullException(nameof(agentUserId));

        var user = await _users.GetFirstOrDefaultAsync(u => u.Id == agentUserId, asNoTracking : true)
                   ?? throw new ArgumentException("Agent not found", nameof(agentUserId));

        const decimal SA_DEFAULT = 10m;
        const decimal AG_DEFAULT = 4m;

        // ── Суперагент: только SA, AG=0
        if (user.Role == UserRoleDb.SuperAgent)
        {
            var saPctSelf = await _repo.GetPercentAsync(FeeType.SuperAgent, user.Id);
            var sa = saPctSelf ?? SA_DEFAULT;
            return (sa, 0m);
        }

        // ── Уровень Агента: если что-то задано — используем для обоих
        var agPctAgent = await _repo.GetPercentAsync(FeeType.Agent, user.Id);       // может быть null
        var saPctAgent = await _repo.GetPercentAsync(FeeType.SuperAgent, user.Id);  // может быть null
        if (agPctAgent.HasValue || saPctAgent.HasValue)
        {
            var sa = saPctAgent ?? SA_DEFAULT;
            var ag = agPctAgent ?? AG_DEFAULT;
            return (sa, ag);
        }

        // ── Уровень Суперагента (родителя): берём только SA, AG дефолт
        if (user.CreatedById is int superAgentId)
        {
            var saPctSuper = await _repo.GetPercentAsync(FeeType.SuperAgent, superAgentId);
            if (saPctSuper.HasValue)
                return (saPctSuper.Value, AG_DEFAULT);
        }

        // ── Дефолты
        return (SA_DEFAULT, AG_DEFAULT);
    }

    public async Task<bool> UpdateAsync(FeeUpdateRequest req)
    {
        if (req.Percent < 0 || req.Percent > 100)
            throw new ArgumentOutOfRangeException(nameof(req.Percent), "Percent must be 0..100");

        var me = await _users.GetFirstOrDefaultAsync(u => u.Id == _current.GetCurrentUserId());
        var now = DateTime.UtcNow;

        switch (req.Type)
        {
            case FeeType.SuperAgent:
                // only for Admin
                if (me?.Role != UserRoleDb.Admin) throw new UnauthorizedAccessException();
                if (req.TargetId is null) throw new ArgumentException("targetId required for SuperAgent");
                var sa = await _users.GetFirstOrDefaultAsync(u => u.Id == req.TargetId);
                if (sa is null || sa.Role != UserRoleDb.SuperAgent) throw new ArgumentException("Invalid superAgent target");
                await _repo.UpsertAsync(FeeType.SuperAgent, req.TargetId, req.Percent, me.Id, now);
                return true;

            case FeeType.Agent:
                // Admin; SuperAgent — only for his agents
                if (req.TargetId is null) throw new ArgumentException("targetId required for Agent");
                var agent = await _users.GetFirstOrDefaultAsync(u => u.Id == req.TargetId);
                if (agent is null || agent.Role != UserRoleDb.Agent) throw new ArgumentException("Invalid agent target");

                if (me?.Role == UserRoleDb.Admin)
                {
                    await _repo.UpsertAsync(FeeType.Agent, req.TargetId, req.Percent, me.Id, now);
                    return true;
                }

                if (me?.Role == UserRoleDb.SuperAgent)
                {
                    var myAgents = await _users.GetAgentsBySuperAgentIdAsync(me.Id);
                    if (!myAgents.Any(a => a.Id == req.TargetId))
                        throw new UnauthorizedAccessException("Not your agent");

                    await _repo.UpsertAsync(FeeType.Agent, req.TargetId, req.Percent, me.Id, now);
                    return true;
                }

                throw new UnauthorizedAccessException();

            default:
                throw new ArgumentOutOfRangeException(nameof(req.Type));
        }
    }
}
