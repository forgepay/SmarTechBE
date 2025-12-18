using System.Globalization;
using System.Numerics;
using CryptoOnRamp.BLL.Clients.TgRampSmart.Internal;
using CryptoOnRamp.BLL.Extensions;
using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using CryptoOnRamp.DAL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;
using MicPic.Infrastructure.Exceptions;
using Microsoft.Extensions.Options;

namespace CryptoOnRamp.BLL.Services.TransactionViaContract.Internal;

internal sealed class TransactionViaContractService(
    ITgRampSmartClient tgRampSmartClient,
    IFeeService feeService,
    ITransactionRepository transactionRepository,
    IUserRepository userRepository,
    IOptionsSnapshot<TransactionViaContractServiceOptions> optionsSnapshot) : ITransactionViaContractService
{
    private readonly ITgRampSmartClient _tgRampSmartClient = tgRampSmartClient;
    private readonly IFeeService _feeService = feeService;

    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly TransactionViaContractServiceOptions _options = optionsSnapshot.Value;


    #region ITgRampSmartClient

    public async Task<TransactionDb> CreateTransactionAsync(
        int superAgentId, string superAgentWallet,
        string fiatCurrency, decimal amount,
        CancellationToken cancellationToken)
    {
        // get admin user

        var adminUser = await _userRepository
            .GetAdminAsync(cancellationToken);

        // checks

        if (string.IsNullOrWhiteSpace(superAgentWallet))
            throw new ArgumentException("Super agent wallet is required.", nameof(superAgentWallet));

        if (string.IsNullOrWhiteSpace(fiatCurrency))
            throw new ArgumentException("Fiat currency is required.", nameof(fiatCurrency));

        if (amount <= 0M)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be > 0");

        if (string.IsNullOrWhiteSpace(adminUser.UsdcWallet))
            throw new AppException("Admin user has no wallet configured", BusinessErrorCodes.Misconfigured);

        // get fees

        var (superAgentFeePercent, _) = await _feeService
            .ResolvePercentsForAgentAsync(superAgentId);

        // generate key

        var apiKey = await _tgRampSmartClient
            .GenerateApiKeyAsync(default);

        if (string.IsNullOrWhiteSpace(apiKey.ApiKey))
            throw new AppException("Failed to generate API key for OnRamp transaction", BusinessErrorCodes.NoData);

        // create payout

        var payout = await _tgRampSmartClient
            .CreatePayoutAsync(
                apiKey: apiKey.ApiKey,
                request: new()
                {
                    Parties = [
                        new()
                        {
                            Address = adminUser.UsdcWallet,
                            Percentage = superAgentFeePercent,
                        },
                        new()
                        {
                            Address = superAgentWallet,
                            Percentage = 100M - superAgentFeePercent,
                        },
                    ],
                },
                cancellationToken: cancellationToken);

        // create transaction

        var tx = new TransactionDb
        {
            UserId = superAgentId,
            Provider = "Onramper",
            FiatCurrency = fiatCurrency.Trim().ToUpperInvariant(),
            FiatAmount = amount,
            UserWallet = superAgentWallet,
            UniqueWalletAddress = payout.Address,
            ContractApiKey = apiKey.ApiKey,
            ContractUniqueId = payout.Id,
            Status = TransactionStatusDb.Issued,
            Timestamp = DateTime.UtcNow,
            SuperAgentPercent = superAgentFeePercent,
            AgentPercent = 0,
            SuperAgentFee = 0,
            AgentFee = 0,
            NetPayout = 0,
            Payouts = [],
            CheckoutSessions = []
        };

        await _transactionRepository
            .InsertAsync(tx);

        await _transactionRepository
            .SaveAsync();

        return tx;
    }

    public async Task<TransactionDb> CreateTransactionAsync(
        int superAgentId, string superAgentWallet,
        int agentId, string agentWallet,
        string fiatCurrency, decimal amount,
        CancellationToken cancellationToken)
    {
        // get admin user

        var adminUser = await _userRepository
            .GetAdminAsync(cancellationToken);

        // check superagent and agent are different

        if (superAgentId == agentId)
            return await CreateTransactionAsync(superAgentId, superAgentWallet, fiatCurrency, amount, cancellationToken);

        // checks

        if (string.IsNullOrWhiteSpace(superAgentWallet))
            throw new ArgumentException("Super agent wallet is required.", nameof(superAgentWallet));

        if (string.IsNullOrWhiteSpace(agentWallet))
            throw new ArgumentException("Agent wallet is required.", nameof(agentWallet));

        if (string.IsNullOrWhiteSpace(fiatCurrency))
            throw new ArgumentException("Fiat currency is required.", nameof(fiatCurrency));

        if (amount <= 0M)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be > 0");

        if (string.IsNullOrWhiteSpace(adminUser.UsdcWallet))
            throw new AppException("Admin user has no wallet configured", BusinessErrorCodes.Misconfigured);

        // get fees

        var (superAgentFeePercent, agentFeePercent) = await _feeService
            .ResolvePercentsForAgentAsync(agentId);

        // generate key

        var apiKey = await _tgRampSmartClient
            .GenerateApiKeyAsync(default);

        if (string.IsNullOrWhiteSpace(apiKey.ApiKey))
            throw new AppException("Failed to generate API key for OnRamp transaction", BusinessErrorCodes.NoData);

        // create payout

        var payout = await _tgRampSmartClient
            .CreatePayoutAsync(
                apiKey: apiKey.ApiKey,
                request: new()
                {
                    Token = "USDT",
                    Parties = [
                        new()
                        {
                            Address = adminUser.UsdcWallet,
                            Percentage = superAgentFeePercent,
                        },
                        new()
                        {
                            Address = superAgentWallet,
                            Percentage = agentFeePercent,
                        },
                        new()
                        {
                            Address = agentWallet,
                            Percentage = 100M - superAgentFeePercent - agentFeePercent,
                        },
                    ],
                },
                cancellationToken: cancellationToken);

        // create transaction

        var tx = new TransactionDb
        {
            UserId = agentId,
            Provider = "Onramper",
            FiatCurrency = fiatCurrency.Trim().ToUpperInvariant(),
            FiatAmount = amount,
            UserWallet = agentWallet,
            UniqueWalletAddress = payout.Address,
            ContractApiKey = apiKey.ApiKey,
            ContractUniqueId = payout.Id,
            Status = TransactionStatusDb.Issued,
            Timestamp = DateTime.UtcNow,
            SuperAgentPercent = superAgentFeePercent,
            AgentPercent = agentFeePercent,
            SuperAgentFee = 0,
            AgentFee = 0,
            NetPayout = 0,
            Payouts = [],
            CheckoutSessions = []
        };

        await _transactionRepository
            .InsertAsync(tx);

        await _transactionRepository
            .SaveAsync();

        return tx;
    }


    public async Task<PayoutResponse> PayoutAsync(int txId, CancellationToken cancellationToken)
    {
        var tx = await _transactionRepository
            .GetByIdAsync(txId);

        // checks

        if (tx is null)
            throw new ArgumentException($"Transaction '{txId}' not found", nameof(txId));

        if (tx.Status is not (TransactionStatusDb.Completed or TransactionStatusDb.Pending))
            throw new InvalidOperationException($"Payout not allowed for status {tx.Status}");

        if (!tx.ContractUniqueId.HasValue)
            throw new InvalidOperationException("Payout not configured for this transaction");

        if (string.IsNullOrWhiteSpace(tx.ContractApiKey))
            throw new InvalidOperationException("Payout not configured for this transaction");

        // claim

        var payout = await _tgRampSmartClient
            .ClaimPayoutAsync(
                apiKey: tx.ContractApiKey,
                id: tx.ContractUniqueId.Value,
                cancellationToken: cancellationToken);

        if (string.IsNullOrWhiteSpace(payout.TxHash))
            throw new AppException("Failed to claim payout via OnRamp", BusinessErrorCodes.NoData);

        if (string.IsNullOrWhiteSpace(payout.Token))
            throw new AppException("Failed to claim payout via OnRamp", BusinessErrorCodes.NoData);

        // record payouts

        tx.Payouts ??= [];

        var totalPaid = BigInteger.Zero;
        var totalFee = BigInteger.Zero;

        for (int i = 0; i < payout.Parties.Count; i++)
        {
            var party = payout.Parties[i];
            var partyType = GetPayoutType(i, payout.Parties.Count);

            if (party.Amount.HasValue && !string.IsNullOrWhiteSpace(party.Address))
            {
                tx.Payouts
                    .Add(new PayoutDb
                    {
                        TransactionId = tx.Id,
                        Transaction = tx,
                        Type = partyType,
                        ToWallet = party.Address,
                        Amount = ToTokenAmountDecimal(payout.Token, party.Amount.Value).ToString(CultureInfo.InvariantCulture),
                        CreatetAt = DateTime.UtcNow,
                        CreatetBy = tx.UserId.ToString(CultureInfo.InvariantCulture),
                        Status = PayoutStatusDb.Completed,
                        TxHash = payout.TxHash,
                    });

                totalPaid += party.Amount.Value;
                totalFee += (i < payout.Parties.Count - 1) ? party.Amount.Value : BigInteger.Zero;
            }
        }

        _transactionRepository.Update(tx);

        await _transactionRepository
            .SaveAsync();

        return new(
            TxHash: payout.TxHash,
            NetAmount: ToTokenAmountDecimal(payout.Token, totalPaid - totalFee),
            FeesDeducted: ToTokenAmountDecimal(payout.Token, totalFee));
    }

    #endregion


    #region Private

    private static decimal ToTokenAmountDecimal(string token, BigInteger amount)
    {
        // only USDC for now
        return (decimal)amount / 1_000_000M;
    }

    public static PayoutType GetPayoutType(int i, int count)
    {
        if (i == 0)
            return PayoutType.Company;

        if (i > 0 && i < count - 1)
            return PayoutType.SuperAgent;

        if (i == count - 1)
            return PayoutType.Agent;

        return PayoutType.Unknown;
    }

    #endregion
}