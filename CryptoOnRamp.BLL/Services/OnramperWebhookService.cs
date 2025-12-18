using CryptoOnRamp.BLL.Helpers;
using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using CryptoOnRamp.BLL.Services.TransactionViaContract;
using CryptoOnRamp.DAL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CryptoOnRamp.BLL.Services;

public class OnramperWebhookService(
    IOptions<OnramperOptions> options,
    IOptions<OnramperWebhookOptions> webhookOptions,
    ITransactionRepository txRepo,
    ICheckoutSessionRepository sessRep,
    ILogger<OnramperWebhookService> logger,
    ITelegramTransactionNotificationService txNotify,
    IWebhookTransactionNotificationService webhookTransactionNotificationService,
    ITransactionViaContractService transactionViaContractService) : IOnramperWebhookService
{
    private readonly OnramperOptions _options = options.Value;
    private readonly OnramperWebhookOptions _webhookOptions = webhookOptions.Value;
    private readonly ITransactionRepository _txRepo = txRepo;
    private readonly ICheckoutSessionRepository _sessRepo = sessRep;
    private readonly ILogger<OnramperWebhookService> _logger = logger;
    private readonly ITelegramTransactionNotificationService _txNotify = txNotify;
    private readonly IWebhookTransactionNotificationService _webhookTransactionNotificationService = webhookTransactionNotificationService;
    private readonly ITransactionViaContractService _transactionViaContractService = transactionViaContractService;

    public async Task ProcessAsync(string rawBody, string correlationId)
    {
        _logger.LogInformation("📥 [{CorrelationId}] Start webhook processing", correlationId);

        // log raw body
        _logger.LogInformation("📦 [{CorrelationId}] Raw webhook body: {RawBody}", correlationId, rawBody);

        // 2) Parse payload
        var payload = JsonSerializer.Deserialize<OnramperWebhookPayload>(rawBody)
                      ?? throw new ArgumentException("Invalid payload");

        _logger.LogInformation("🔎 [{CorrelationId}] Parsed payload: TxId={TxId}, Status={Status}, PartnerContext={PartnerContext}",
            correlationId, payload.TransactionId, payload.Status, payload.PartnerContext);

        // 3) Optional apiKey guard
        if (!string.IsNullOrEmpty(_webhookOptions.AllowedApiKey) &&
            !string.Equals(payload.ApiKey, _webhookOptions.AllowedApiKey, StringComparison.Ordinal))
        {
            _logger.LogWarning("⛔ [{CorrelationId}] Invalid API key. Provided={ApiKey}", correlationId, payload.ApiKey);
            throw new UnauthorizedAccessException("Invalid API key");
        }

        // 4) Map status
        var newStatus = MapStatus(payload.Status);
        _logger.LogInformation("📌 [{CorrelationId}] Mapped Onramper status {Incoming} -> {Mapped}",
            correlationId, payload.Status, newStatus);

        // 5) Resolve transactionId + sessionIdx from PartnerContext: "txId:idx"
        int txId;
        int? sessionIdx = null;
        if (string.IsNullOrWhiteSpace(payload.PartnerContext))
            throw new ArgumentException("Missing partnerContext");

        var parts = payload.PartnerContext.Split(':', 2, StringSplitOptions.TrimEntries);
        if (parts.Length >= 1 && int.TryParse(parts[0], out var parsedTx))
            txId = parsedTx;
        else
            throw new ArgumentException("Invalid partnerContext format (txId missing)");

        if (parts.Length == 2 && int.TryParse(parts[1], out var parsedIdx))
            sessionIdx = parsedIdx;

        _logger.LogInformation("🧩 [{CorrelationId}] PartnerContext resolved: TxId={TxId}, SessionIdx={SessionIdx}",
            correlationId, txId, sessionIdx);

        // 6) Load transaction
        var tx = await _txRepo.GetByIdWithAllMetadataAsync(txId)
                 ?? throw new KeyNotFoundException("Transaction not found");

        _logger.LogInformation("📂 [{CorrelationId}] Loaded transaction from DB. CurrentStatus={Status}, ExternalId={ExternalId}",
            correlationId, tx.Status, tx.ExternalId);

        // 7) Update checkout session if present
        CheckoutSessionDb? session = null;
        if (sessionIdx.HasValue)
        {
            session = await _sessRepo.GetFirstOrDefaultAsync(
                s => s.TransactionId == tx.Id && s.PartnerContext == payload.PartnerContext);

            if (session != null)
            {
                if (!string.IsNullOrWhiteSpace(payload.TransactionId))
                    session.ExternalId = payload.TransactionId;

                session.Status = newStatus; // e.g. issued/pending/completed/failed
                _sessRepo.Update(session);

                _logger.LogInformation("📝 [{CorrelationId}] Updated checkout session. SessionId={SessionId}, NewStatus={Status}",
                    correlationId, session.Id, newStatus);
            }
            else
            {
                _logger.LogWarning("⚠️ [{CorrelationId}] Session not found for PartnerContext={PartnerContext}",
                    correlationId, payload.PartnerContext);
            }
        }

        // 8) Idempotency & do-not-downgrade
        var updatedTxStatus = MergeStatus(tx.Status, newStatus);

        if (!string.IsNullOrWhiteSpace(payload.TransactionId) && string.IsNullOrWhiteSpace(tx.ExternalId))
            tx.ExternalId = payload.TransactionId;

        tx.Status = updatedTxStatus;
        tx.Timestamp = DateTime.UtcNow;


        tx.CryptoAmount = payload.OutAmount;
        FeeCalculator.ApplyFees(tx);
        _txRepo.Update(tx);
        if (session != null) _sessRepo.Update(session);

        await _txRepo.SaveAsync();

        if (updatedTxStatus == TransactionStatusDb.Completed)
        {
            await _transactionViaContractService.PayoutAsync(tx.Id, default);
            await _webhookTransactionNotificationService.NotifyTransactionCompletedAsync(tx.Id);
            await _txNotify.NotifyTransactionCompletedAsync(tx.Id);
        }

        _logger.LogInformation("✅ [{CorrelationId}] Transaction updated. TxId={TxId}, FinalStatus={Status}",
            correlationId, tx.Id, updatedTxStatus);
    }

    public bool ValidateSignature(string rawBody, string signature)
    {
        if (string.IsNullOrWhiteSpace(signature))
        {
            _logger.LogWarning("Onramper webhook signature is empty");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_options.WebHookSecret))
        {
            _logger.LogError("Onramper secret is not configured");
            return false;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.WebHookSecret));
        
        var expected = Convert.ToHexString(
            inArray: hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody)));
        
        return string.Equals(signature, expected, StringComparison.OrdinalIgnoreCase);
    }

    private static TransactionStatusDb MapStatus(OnramperTransactionStatus s) => s switch
    {
        OnramperTransactionStatus.New => TransactionStatusDb.Pending,
        OnramperTransactionStatus.Pending => TransactionStatusDb.Pending,
        OnramperTransactionStatus.Paid => TransactionStatusDb.Completed, // получен фиат
        OnramperTransactionStatus.Failed => TransactionStatusDb.Failed,
        OnramperTransactionStatus.Canceled => TransactionStatusDb.Failed,
        _ => TransactionStatusDb.Pending
    };

    // не даём "понизить" статус транзакции повторным вебхуком
    private static TransactionStatusDb MergeStatus(TransactionStatusDb current, TransactionStatusDb incoming)
    {
        // порядок приоритета: Failed > Completed > Pending > Issued
        int Rank(TransactionStatusDb s) => s switch
        {
            TransactionStatusDb.Failed => 4,
            TransactionStatusDb.Completed => 3,
            TransactionStatusDb.Pending => 2,
            TransactionStatusDb.Issued => 1,
            TransactionStatusDb.PayoutCompleted => 5, // если уже сделали payout — не трогаем
            _ => 0
        };

        // не трогаем, если уже payout
        if (current == TransactionStatusDb.PayoutCompleted) return current;

        return Rank(incoming) >= Rank(current) ? incoming : current;
    }

    private static string MapSessionStatus(TransactionStatusDb txStatus) => txStatus switch
    {
        TransactionStatusDb.Pending => "pending",
        TransactionStatusDb.Completed => "completed",
        TransactionStatusDb.Failed => "failed",
        _ => "issued"
    };
}
