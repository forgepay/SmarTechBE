using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CryptoOnRamp.BLL.Interfaces;

public interface IBotClient
{
    ITelegramBotClient GetClient();
    Task<Message> SendTextMessageAsync(ChatId chatId, string text, IReplyMarkup? replyMarkup = null, CancellationToken cancellationToken = default);
}
