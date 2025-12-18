using CryptoOnRamp.BLL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CryptoOnRamp.BLL.Services;

public class TelegramBotService(ILogger<TelegramBotService> logger, ITelegramBotClient botClient, IServiceScopeFactory scopeFactory) : IHostedService
{
    private readonly ILogger<TelegramBotService> _logger = logger;
    private readonly ITelegramBotClient _botClient = botClient;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private CancellationTokenSource? _cts;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🚀 Telegram Bot starting with polling...");

        _cts = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        Task.Run(() => _botClient.ReceiveAsync(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            _cts.Token
        ), _cts.Token);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🛑 Telegram Bot stopping...");
        _cts?.Cancel();
        return Task.CompletedTask;
    }

    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
    {
        // 1) Inline-кнопки (CallbackQuery)
        if (update.CallbackQuery is { } cb && cb.Message is { } msg)
        {
            var chatId = msg.Chat.Id;
            var data = cb.Data ?? string.Empty;

            _logger.LogInformation("🖱️ Callback from {User}: {Data}", cb.From?.Username, data);

            using var scope = _scopeFactory.CreateScope();
            var dialog = scope.ServiceProvider.GetRequiredService<ITelegramBotDialogService>();

            await dialog.HandleCallbackAsync(new BotClientAdapter(bot), chatId, data, token);

            // подтвердим нажатие, чтобы убрать «крутилку»
            await bot.AnswerCallbackQueryAsync(cb.Id, cancellationToken: token);
            return;
        }

        // 2) Обычные текстовые сообщения
        if (update.Message is { Text: { } messageText })
        {
            var chatId = update.Message.Chat.Id;

            _logger.LogInformation("📩 Message from {User}: {Text}", update.Message.From?.Username, messageText);

            using var scope = _scopeFactory.CreateScope();
            var dialog = scope.ServiceProvider.GetRequiredService<ITelegramBotDialogService>();

            try
            {
                await dialog.HandleDialogAsync(new BotClientAdapter(bot), chatId, messageText, token);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return;
        }

        // 3) Остальные типы апдейтов можно игнорировать/логировать
        _logger.LogDebug("Ignoring update type: {Type}", update.Type);
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
    {
        _logger.LogError(exception, "❌ error in Polling");
        return Task.CompletedTask;
    }
}
