namespace CryptoOnRamp.BLL.Interfaces;

public interface IWebhookTransactionNotificationService
{
    Task NotifyTransactionCompletedAsync(int transactionId, CancellationToken ct = default);
}
