using CryptoOnRamp.BLL.Extensions;
using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using CryptoOnRamp.DAL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;

namespace CryptoOnRamp.BLL.Services;


public sealed class GenerateLinkService(ILinkGenerationOrchestrator orchestrator, IUserService currentUser, IUserRepository userRepository) : IGenerateLinkService
{
    private readonly ILinkGenerationOrchestrator _orchestrator = orchestrator;
    private readonly IUserService _currentUser = currentUser;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<GenerateLinkResponse> GenerateAsync(GenerateLinkRequest req, CancellationToken ct)
    {
        if (req.Amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(req.Amount), "Amount must be positive.");
        
        if (string.IsNullOrWhiteSpace(req.FiatCurrency))
            throw new ArgumentException("FiatCurrency is required.", nameof(req.FiatCurrency));

        var fiat = req.FiatCurrency
            .Trim()
            .ToUpperInvariant();

        var requestorId = _currentUser
            .GetCurrentUserId();
        
        var currentUser = await _userRepository
            .GetActiveUserAsync(requestorId, default);

        var targetUser = await ResolveTargetUserAsync(currentUser, req, ct);

        var input = new LinkGenerationInput
        {
            RequestorUserId = requestorId,
            TargetUserId = targetUser.Id,
            FiatCurrency = fiat,
            Amount = req.Amount,
            Email = targetUser.Email
        };

        var result = await _orchestrator.GenerateAsync(input, ct);

        return new GenerateLinkResponse
        {
            TransactionId = result.TransactionId,
            PaymentLinks = result.PaymentLinks,
            UniqueWalletAddress = result.UniqueWalletAddress,
        };
    }

    private async Task<UserDb> ResolveTargetUserAsync(UserDb currentUser, GenerateLinkRequest req, CancellationToken ct)
    {
        switch (currentUser.Role)
        {
            case UserRoleDb.Agent:
                return currentUser;

            case UserRoleDb.Admin:
                {
                    var targetId = req.UserId
                        ?? throw new ArgumentException("UserId is required for Admin.", nameof(req.UserId));

                    return await _userRepository
                        .GetActiveUserAsync(targetId, default);
                }

            case UserRoleDb.SuperAgent:
                {
                    var targetId = req.UserId
                        ?? throw new ArgumentException("UserId is required for SuperAgent.", nameof(req.UserId));
                    
                    var u = await _userRepository
                        .GetActiveUserAsync(targetId, default);

                    if (u.CreatedById != currentUser.Id && u.Id != currentUser.Id)
                        throw new ArgumentException("This user is not your agent.", nameof(req.UserId));

                    return u;
                }

            default:
                throw new NotSupportedException($"Role '{currentUser.Role}' is not supported for link generation.");
        }
    }
}