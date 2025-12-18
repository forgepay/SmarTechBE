using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using CryptoOnRamp.DAL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;

namespace CryptoOnRamp.BLL.Services;

public class StatsService(ITransactionRepository txRepo, IUserRepository userRepo, IUserService currentUser) : IStatsService
{
    private readonly ITransactionRepository _transactionRepository = txRepo;
    private readonly IUserRepository _userRepository = userRepo;
    private readonly IUserService _userService = currentUser;

    public async Task<GlobalStatsDto> GetGlobalAsync()
    {
        var currentUser = await _userRepository.GetFirstOrDefaultAsync(u => u.Id == _userService.GetCurrentUserId())
                 ?? throw new UnauthorizedAccessException("User not found");

        IEnumerable<TransactionDb> txs = new List<TransactionDb>();


        if (currentUser.Role == UserRoleDb.Agent)
        {
            txs = await _transactionRepository.GetTransactionsAsync(
                userId: currentUser.Id, status: null, dateFrom: null, dateTo: null);
        }
        if (currentUser.Role == UserRoleDb.SuperAgent)
        {
            var agents = await _userRepository.GetAgentsBySuperAgentIdAsync(currentUser.Id);
            var agentIds = agents.Select(x => x.Id).ToList();
            var allowedUserIds = agentIds.Append(currentUser.Id).ToList();
            txs = await _transactionRepository.GetTransactionsAsync(
                userId: currentUser.Id, status: null, dateFrom: null, dateTo: null, allowedUserIds: allowedUserIds);
        }
        if (currentUser.Role == UserRoleDb.Admin)
        {
            txs = await _transactionRepository.GetTransactionsAsync(
                userId: null, status: null, dateFrom: null, dateTo: null);
        }

        return Aggregate(txs);
    }

    public async Task<UserStatsDto> GetUserAsync(int userId)
    {
        var currentUser = await _userRepository.GetFirstOrDefaultAsync(u => u.Id == _userService.GetCurrentUserId())
                 ?? throw new UnauthorizedAccessException("User not found");

        // authorize scope:
        // - SuperAgent: self or any of own agents
        // - Admin: any user
        if (currentUser.Role == UserRoleDb.Agent)
            throw new UnauthorizedAccessException("Agents cant view stats");

        if (currentUser.Role == UserRoleDb.SuperAgent && currentUser.Id != userId)
        {
            var myAgents = await _userRepository.GetAgentsBySuperAgentIdAsync(currentUser.Id);
            if (!myAgents.Any(a => a.Id == userId))
                throw new UnauthorizedAccessException("Not your agent");
        }

        var user = await _userRepository.GetFirstOrDefaultAsync(u => u.Id == userId)
                   ?? throw new ArgumentException("User not found");

        List<int> scopeIds = new() { userId };
        List<UserStatsItemDto> subUsers = new();

        if (user.Role == UserRoleDb.SuperAgent || user.Role == UserRoleDb.Admin)
        {
            var agents = await _userRepository.GetAgentsBySuperAgentIdAsync(user.Id);
            var agentIds = agents.Select(a => a.Id).ToList();
            scopeIds.AddRange(agentIds);

            foreach (var agent in agents)
            {
                var agentTxs = await _transactionRepository.GetTransactionsAsync(
                    userId: agent.Id, status: null, dateFrom: null, dateTo: null);
                subUsers.Add(new UserStatsItemDto(agent.Id, Aggregate(agentTxs)));
            }
        }

        var txsAll = await _transactionRepository.GetTransactionsAsync(
            userId: null, status: null, dateFrom: null, dateTo: null, allowedUserIds: scopeIds);

        var total = Aggregate(txsAll);

        return new UserStatsDto(
            UserId: userId,
            TotalIssued: total.TotalIssued,
            TotalCompleted: total.TotalCompleted,
            TotalPending: total.TotalPending,
            TotalFailed: total.TotalFailed,
            TotalVolume: total.TotalVolume,
            SubUsers: subUsers
        );
    }

    private static GlobalStatsDto Aggregate(IEnumerable<TransactionDb> txs)
    {
        int issued = 0, completed = 0, pending = 0, failed = 0;
        decimal volume = 0;

        foreach (var t in txs)
        {
            volume += t.FiatAmount;

            switch (t.Status)
            {
                case TransactionStatusDb.Issued: issued++; break;
                case TransactionStatusDb.Completed: completed++; break;
                case TransactionStatusDb.Pending: pending++; break;
                case TransactionStatusDb.Failed: failed++; break;
            }
        }

        return new GlobalStatsDto(
            TotalIssued: issued,
            TotalCompleted: completed,
            TotalPending: pending,
            TotalFailed: failed,
            TotalVolume: volume
        );
    }
}