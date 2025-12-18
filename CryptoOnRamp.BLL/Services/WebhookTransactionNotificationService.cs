using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace CryptoOnRamp.BLL.Services;

internal sealed class WebhookTransactionNotificationService(
    ITransactionRepository txRepo,
    IWebHookService webHookService,
    ILogger<WebhookTransactionNotificationService> log) : IWebhookTransactionNotificationService
{
    private readonly ITransactionRepository _txRepo = txRepo;
    private readonly IWebHookService _webHookService = webHookService;
    private readonly ILogger<WebhookTransactionNotificationService> _log = log;

    public async Task NotifyTransactionCompletedAsync(int transactionId, CancellationToken ct = default)
    {
        var tx = await _txRepo.GetByIdAsync(transactionId);

        if (tx is null)
        {
            _log.LogWarning("TX notify: transaction {Id} not found", transactionId);
            return;
        }

        try
        {
            var sent = await _webHookService
                .CallWebhookEndpointAsync(
                    userId: tx.UserId,
                    action: "TransactionCompleted",
                    payload: new
                    {
                        TransactionId = tx.Id,
                        tx.FiatCurrency,
                        tx.FiatAmount,
                        tx.CryptoCurrency,
                        tx.CryptoAmount,
                        tx.UserWallet,
                        tx.TxHash,
                        tx.Status,
                    },
                    cancellationToken: ct);

            if (sent)
            {
                _log.LogInformation("TX notify: transaction {Id} notification sent to user {UserId}", transactionId, tx.UserId);
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "TX notify: failed to notify user {UserId} about transaction {Id}", tx.UserId, transactionId);
        }
    }

}
