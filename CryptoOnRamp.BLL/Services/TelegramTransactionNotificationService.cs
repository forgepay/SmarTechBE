using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Localization;
using CryptoOnRamp.BLL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace CryptoOnRamp.BLL.Services;

public sealed class TelegramTransactionNotificationService(
    ITransactionRepository txRepo,
    IUserRepository userRepo,
    ITelegramUserRepository telegramUserRepository,
    ITelegramBotClient bot,
    ILogger<TelegramTransactionNotificationService> log,
    IOptions<AppilcationSettings> appOptions) : ITelegramTransactionNotificationService
{
    private readonly ITransactionRepository _txRepo = txRepo;
    private readonly IUserRepository _userRepo = userRepo;
    private readonly ITelegramUserRepository _telegramUserRepository = telegramUserRepository;
    private readonly ITelegramBotClient _bot = bot;   // зарегистрируй в DI
    private readonly ILogger<TelegramTransactionNotificationService> _log = log;
    private readonly AppilcationSettings _appOptions = appOptions.Value;

    public async Task NotifyTransactionCompletedAsync(int transactionId, CancellationToken ct = default)
    {
        var tx = await _txRepo.GetByIdAsync(transactionId);
        if (tx is null)
        {
            _log.LogWarning("TX notify: transaction {Id} not found", transactionId);
            return;
        }

        var user = await _userRepo.GetByIdAsync(tx.UserId);
        if (user is null)
        {
            _log.LogWarning("TX notify: user {UserId} not found for tx {TxId}", tx.UserId, tx.Id);
            return;
        }

        var telegramUsers = await _telegramUserRepository.GetAllByUserIdAsync(user.Id, ct);
        if (telegramUsers.Count == 0)
        {
            _log.LogInformation("TX notify: user {UserId} has no TelegramId; skip", user.Id);
            return;
        }

        var messageTemplate = I18n.Text["transaction_completed"][user.Language];
        var message = messageTemplate.Replace("{url}", _appOptions.UrlUi + "dashboard");

        foreach (var tgUser in telegramUsers)
        {
            await _bot.SendTextMessageAsync(
                    chatId: tgUser.TelegramId,
                    text: message,
                    cancellationToken: ct);
    
            _log.LogInformation("TX notify: sent to user {UserId} (tg {Tg}) for tx {TxId}",
                user.Id, tgUser.TelegramId, tx.Id);                
        }
    }
}
