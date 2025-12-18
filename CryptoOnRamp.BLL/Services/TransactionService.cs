using AutoMapper;
using CryptoOnRamp.BLL.Extensions;
using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using CryptoOnRamp.DAL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;

namespace CryptoOnRamp.BLL.Services;

public class TransactionService(
    ITransactionRepository transactionRepository,
    IUserRepository userRepository,
    IUserService currentUser,
    IMapper mapper,
    ICheckoutSessionRepository checkoutSessionRepository,
    IPayoutRepository payoutRepository) : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUserService _currentUser = currentUser;
    private readonly IMapper _mapper = mapper;
    private readonly ICheckoutSessionRepository _checkoutSessionRepository = checkoutSessionRepository;
    private readonly IPayoutRepository _payoutRepository = payoutRepository;

    public async Task<IEnumerable<TransactionDto>> GetTransactionsAsync(int? userId, TransactionStatus? status, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize)
    {
        var currentUserId = _currentUser
            .GetCurrentUserId();

        var currentUser = await _userRepository
            .GetActiveUserAsync(currentUserId, default);

        IEnumerable<TransactionDb> transactions;

        switch (currentUser?.Role)
        {
            case UserRoleDb.Admin:
                
                if (userId.HasValue)
                {
                    _ = await _userRepository
                        .GetActiveUserAsync(userId.Value, default);
                }

                transactions = await _transactionRepository
                    .GetTransactionsAsync(userId, status.HasValue ? (TransactionStatusDb?)(TransactionStatusDb)status.Value : null, dateFrom, dateTo, null, page, pageSize);

                break;

            case UserRoleDb.SuperAgent:

                var agents = await _userRepository
                    .GetAgentsBySuperAgentIdAsync(currentUser.Id);

                var agentIds = agents
                    .Select(x => x.Id)
                    .ToList();

                var allowedUserIds = agentIds
                    .Append(currentUser.Id)
                    .ToList();

                transactions = await _transactionRepository
                    .GetTransactionsAsync(userId, status.HasValue ? (TransactionStatusDb?)(TransactionStatusDb)status.Value : null, dateFrom, dateTo, allowedUserIds, page, pageSize);

                break;

            case UserRoleDb.Agent:
                
                transactions = await _transactionRepository
                    .GetTransactionsAsync(currentUser.Id, status.HasValue ? (TransactionStatusDb?)(TransactionStatusDb)status.Value : null, dateFrom, dateTo, null, page, pageSize);
                
                break;

            default:
                throw new UnauthorizedAccessException("Invalid role");
        }

        return _mapper
            .Map<IEnumerable<TransactionDto>>(transactions);
    }

    private async Task EnsurePayoutAccessAsync(TransactionDb tx)
    {
        var currentUserId = _currentUser
            .GetCurrentUserId();
        
        var currentUser = await _userRepository
            .GetActiveUserAsync(currentUserId, default);

        switch (currentUser?.Role)
        {
            case UserRoleDb.Admin:
                break;

            case UserRoleDb.SuperAgent:

                var myAgents = await _userRepository
                    .GetAgentsBySuperAgentIdAsync(currentUser.Id);
                
                if (!myAgents.Any(a => a.Id == tx.UserId) && tx.UserId != currentUser.Id)
                    throw new UnauthorizedAccessException("Not allowed");

                break;

            case UserRoleDb.Agent:

                if (tx.UserId != currentUser.Id)
                    throw new UnauthorizedAccessException("Not allowed");

                break;

            default:
                throw new UnauthorizedAccessException("Invalid role");
        }
    }

    public async Task<CheckoutSessionDTO?> GetSessionByIdAsync(string sessionId)
    {
        var result = await _checkoutSessionRepository.GetByIdAsync(sessionId);

        if (result == null)
        {
            throw new ArgumentException("Session not found");
        }

        return _mapper.Map<CheckoutSessionDTO?>(result);
    }

    public async Task<TransactionDto> GetTransactionAsync(int transactionId)
    {
        var transactionDb = await _transactionRepository
            .GetByIdWithAllMetadataAsync(transactionId) ?? throw new ApplicationException("No transaction found.");
        
        await EnsurePayoutAccessAsync(transactionDb);
        
        return _mapper
            .Map<TransactionDto>(transactionDb);
    }

    public async Task<IEnumerable<PayoutDto>> GetPayoutsAsync(
        int? userId,
        PayoutStatusDb? status,
        DateTime? dateFrom,
        DateTime? dateTo,
        int page,
        int pageSize)
    {
        var currentUserId = _currentUser
            .GetCurrentUserId();

        var currentUser = await _userRepository
            .GetActiveUserAsync(currentUserId, default);

        IEnumerable<PayoutDb> payouts;

        var statusDb = status.HasValue ? (PayoutStatusDb?)(PayoutStatusDb)status.Value : null;

        switch (currentUser.Role)
        {
            case UserRoleDb.Admin:

                payouts = await _payoutRepository
                    .GetPayoutsAsync(userId, statusDb, dateFrom, dateTo, null, page, pageSize);

                break;

            case UserRoleDb.SuperAgent:

                var agents = await _userRepository
                    .GetAgentsBySuperAgentIdAsync(currentUser.Id);

                var allowedUserIds = agents
                    .Select(a => a.Id)
                    .Append(currentUser.Id)
                    .ToList();

                payouts = await _payoutRepository
                    .GetPayoutsAsync(userId, statusDb, dateFrom, dateTo, allowedUserIds, page, pageSize);

                payouts = payouts
                    .Where(x => x.Type == PayoutType.SuperAgent);

                break;

            case UserRoleDb.Agent:

                payouts = await _payoutRepository
                    .GetPayoutsAsync(currentUser.Id, statusDb, dateFrom, dateTo, null, page, pageSize);

                payouts = payouts
                    .Where(x => x.Type == PayoutType.Agent);

                break;

            default:
                throw new UnauthorizedAccessException("Invalid role.");
        }

        return _mapper
            .Map<IEnumerable<PayoutDto>>(payouts);
    }

    public async Task<PayoutDto> GetPayoutAsync(int payoutId)
    {
        var payoutDb = await _payoutRepository
            .GetByIdWithAllMetadataAsync(payoutId) ?? throw new ApplicationException("No payout found.");

        await EnsurePayoutAccessAsync(payoutDb.Transaction);

        return _mapper
            .Map<PayoutDto>(payoutDb);
    }
}
