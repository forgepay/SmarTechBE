using CryptoOnRamp.DAL.Models;

namespace CryptoOnRamp.DAL.Repositories.Interfaces;

public interface IFeeSchemeRepository
{
    Task<decimal?> GetPercentAsync(FeeType type, int? targetUserId);

    Task UpsertAsync(FeeType type, int? targetUserId, decimal percent, int actorUserId, DateTime now);

    Task<List<(int SuperAgentId, decimal Percent)>> GetAllSuperAgentPercentsAsync();
    Task<List<(int AgentId, decimal Percent)>> GetAgentPercentsForSuperAgentAsync(int superAgentId);
    Task<List<(int AgentId, decimal Percent)>> GetAllAgentPercentsAsync();
}
