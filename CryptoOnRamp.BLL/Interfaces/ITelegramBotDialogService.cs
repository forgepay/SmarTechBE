namespace CryptoOnRamp.BLL.Interfaces;

public interface ITelegramBotDialogService
{
    Task HandleDialogAsync(IBotClient bot, long chatId, string messageText, CancellationToken token);
    Task HandleCallbackAsync(IBotClient bot, long chatId, string callbackData, CancellationToken token);
}
