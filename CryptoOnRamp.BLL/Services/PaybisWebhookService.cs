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

public sealed class PaybisWebhookService(
    IOptions<PaybisOptions> opt,
    ITransactionRepository tx,
    ICheckoutSessionRepository sessions,
    ILogger<PaybisWebhookService> logger,
    ITelegramTransactionNotificationService txNotify,
    IWebhookTransactionNotificationService webhookTransactionNotificationService,
    ITransactionViaContractService transactionViaContractService) : IPaybisWebhookService
{
    private readonly PaybisOptions _opt = opt.Value;
    private readonly ITransactionRepository _tx = tx;
    private readonly ICheckoutSessionRepository _sessions = sessions;
    private readonly ILogger<PaybisWebhookService> _logger = logger;
    private readonly ITelegramTransactionNotificationService _txNotify = txNotify;
    private readonly IWebhookTransactionNotificationService _webhookTransactionNotificationService = webhookTransactionNotificationService;
    private readonly ITransactionViaContractService _transactionViaContractService = transactionViaContractService;

    public async Task ProcessAsync(string rawBody, string? correlationId = null)
    {
        correlationId ??= Guid.NewGuid().ToString("N");


        // 2) парсинг payload
        PaybisWebhookPayload payload;
        try
        {
            payload = JsonSerializer.Deserialize<PaybisWebhookPayload>(rawBody)
                      ?? throw new ArgumentException("Empty payload after deserialize");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("🧩 [{cid}] Bad Paybis payload: {err}. Raw={raw}", correlationId, ex.Message, rawBody);
            throw;
        }

        var requestId = payload.Data?.RequestId?.Trim();
        var partnerUserId = payload.Data?.PartnerUserId?.Trim();
        var extStatus = payload.Data?.Transaction?.Status?.Trim();
        var cryptoAmount = Decimal.TryParse(payload.Data?.AmountInfo?.Amount, out var amount) ? amount : 0;
        var cryptoCurrency = payload.Data?.AmountInfo?.Currency?.Trim();


        _logger.LogInformation("🔎 [{cid}] Parsed Paybis payload: requestId={req}, partnerUserId={uid}, status={st}",
            correlationId, requestId ?? "null", partnerUserId ?? "null", extStatus ?? "null");

        if (string.IsNullOrWhiteSpace(requestId))
            throw new ArgumentException("Missing requestId in Paybis payload");

        // 3) поиск сессии
        CheckoutSessionDb? session =
            await _sessions.GetFirstOrDefaultAsync(s => s.ExternalId == requestId);

        if (session is null)
        {
            if (!int.TryParse(partnerUserId, out var txId))
            {
                _logger.LogWarning("❓ [{cid}] Cannot resolve txId from partnerUserId='{uid}'", correlationId, partnerUserId);
                throw new KeyNotFoundException("Cannot resolve txId from partnerUserId");
            }

            session = await _sessions.GetFirstOrDefaultAsync(
                s => s.TransactionId == txId && s.Ramp == "paybis" && s.PartnerContext == $"{txId}:pb");

            if (session is null)
            {
                _logger.LogWarning("🔎 [{cid}] Paybis session not found by txId={txId}", correlationId, txId);
                throw new KeyNotFoundException($"Paybis session not found for txId={txId}");
            }

            // сохраняем ExternalId на будущее
            var before = session.ExternalId;
            session.ExternalId = requestId;
            _sessions.Update(session);
            await _sessions.SaveAsync();

            _logger.LogInformation("📝 [{cid}] Bound Paybis requestId to session: txId={txId}, oldExtId={old}, newExtId={new}",
                correlationId, session.TransactionId, before ?? "null", requestId);
        }
        else
        {
            _logger.LogInformation("🔁 [{cid}] Found session by ExternalId: txId={txId}, sessionId={sid}",
                correlationId, session.TransactionId, session.Id);
        }

        // 4) маппинг статуса и монотонное обновление
        var newStatus = MapPaybisStatus(extStatus ?? string.Empty);
        var oldSessionStatus = session.Status;

        await UpdateMonotonicAsync(session, newStatus, cryptoAmount, cryptoCurrency);

        if (newStatus == TransactionStatusDb.Completed)
        {
            await _transactionViaContractService.PayoutAsync(session.TransactionId, default);
            await _webhookTransactionNotificationService.NotifyTransactionCompletedAsync(session.TransactionId);
            await _txNotify.NotifyTransactionCompletedAsync(session.TransactionId);
        }

        _logger.LogInformation("✅ [{cid}] Status updated (monotonic): session {old} -> {new}",
            correlationId, oldSessionStatus, newStatus);
    }

    public bool ValidateSignature(string rawBody, string signatureBase64)
    {
        if (string.IsNullOrWhiteSpace(signatureBase64))
        {
            _logger.LogWarning("Paybis webhook signature is empty");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_opt.PublicKey))
        {
            _logger.LogError("Paybis public key is not configured");
            return false;
        }

        using RSA rsa = RSA.Create();

        rsa.ImportFromPem(_opt.PublicKey);

        return rsa.VerifyData(
            Encoding.UTF8.GetBytes(rawBody),
            Convert.FromBase64String(signatureBase64),
            HashAlgorithmName.SHA512,
            RSASignaturePadding.Pss
        );
    }

    // --- helpers ---

    private static TransactionStatusDb MapPaybisStatus(string s)
    {

        var v = (s ?? "").Trim().ToLowerInvariant();
        return v switch
        {
            "approved" or "paid" or "completed" or "success" => TransactionStatusDb.Completed,
            "pending" or "in_progress" or "processing" => TransactionStatusDb.Pending,
            "cancelled" or "canceled" => TransactionStatusDb.Failed,
            "failed" or "declined" => TransactionStatusDb.Failed,
            _ => TransactionStatusDb.Pending
        };
    }

    private async Task UpdateMonotonicAsync(CheckoutSessionDb session, TransactionStatusDb newStatus, decimal cryptoAmount, string? cryptoCurrency)
    {
        // 1) session
        if (IsForward(session.Status, newStatus))
        {
            session.Status = newStatus;
            _sessions.Update(session);
        }

        // 2) parent transaction — подтягиваем вверх, но не откатываем назад
        var tx = await _tx.GetByIdWithAllMetadataAsync(session.TransactionId) ?? throw new KeyNotFoundException($"Transaction not found for id={session.TransactionId}");

        if (IsForward(tx.Status, newStatus))
        {
            tx.Status = newStatus;
            _tx.Update(tx);
        }

        tx.CryptoAmount = cryptoAmount;

        FeeCalculator.ApplyFees(tx);

        await _tx.SaveAsync();       // если один контекст Db — этого достаточно
        await _sessions.SaveAsync();  // иначе сохранить обеих
    }

    // Определи упорядочивание статусов под свой жизненный цикл
    private static bool IsForward(TransactionStatusDb current, TransactionStatusDb next)
        => Order(next) >= Order(current);

    private static int Order(TransactionStatusDb s) => s switch
    {
        TransactionStatusDb.Issued => 0,
        TransactionStatusDb.Pending => 1,
        TransactionStatusDb.Completed => 2,
        TransactionStatusDb.PayoutCompleted => 3,
        TransactionStatusDb.Failed => 99,
        _ => 0
    };
}