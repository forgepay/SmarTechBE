using CryptoOnRamp.BLL.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CryptoOnRamp.BLL.Services;

public sealed class BotClientAdapter(ITelegramBotClient inner) : IBotClient
{
    private readonly ITelegramBotClient _inner = inner;

    public ITelegramBotClient GetClient()
    {
        return _inner;
    }

    public Task<Message> SendTextMessageAsync(ChatId chatId, string text, IReplyMarkup? markup = null, CancellationToken ct = default) =>
        _inner.SendTextMessageAsync(chatId, text, replyMarkup: markup, cancellationToken: ct); // под v19
}
