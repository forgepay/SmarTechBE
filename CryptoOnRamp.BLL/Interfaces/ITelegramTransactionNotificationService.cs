namespace CryptoOnRamp.BLL.Interfaces;

public interface ITelegramTransactionNotificationService
{
    Task NotifyTransactionCompletedAsync(int transactionId, CancellationToken ct = default);
}
