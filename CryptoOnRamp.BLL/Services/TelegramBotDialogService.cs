using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Localization;
using CryptoOnRamp.DAL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot.Types.ReplyMarkups;

namespace CryptoOnRamp.BLL.Services;

public class TelegramBotDialogService(IUserRepository userRepository, ILinkGenerationOrchestrator orchestrator, IUserService userService, ITelegramUserService telegramUserService) : ITelegramBotDialogService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUserService _userService = userService;
    private readonly ITelegramUserService _telegramUserService = telegramUserService;
    private readonly ILinkGenerationOrchestrator _orchestrator = orchestrator;

    // ========= ENTRY: text messages =========
    public async Task HandleDialogAsync(IBotClient bot, long chatId, string messageText, CancellationToken token)
    {
        messageText ??= string.Empty;
        var text = messageText.Trim();

        // 0) найти/создать пользователя
        var telegramUser = await _telegramUserService
            .GetByTelegramIdOrDefaultAsync(chatId, token);

        var user = telegramUser is null ? null : await _userRepository.GetByIdAsync(telegramUser.UserId);
        if (user is null)
        {
            user = new UserDb
            {
                Name = $"tg_{chatId}",
                Email = $"tg_{chatId}",
                Phone = string.Empty,
                UsdcWallet = string.Empty,
                Role = UserRoleDb.SuperAgent,
                CreatedAt = DateTime.UtcNow,
                RegistrationStep = RegistrationStep.WaitingLanguage,
                // TelegramId = chatId,
                //Language = AppLanguage.English
            };
            await _userRepository.InsertAsync(user);
            await _userRepository.SaveAsync();

            telegramUser = await _telegramUserService.AddAsync(chatId, user.Id, token);

            await bot.SendTextMessageAsync(
              chatId,
              I18n.Text["choose_language"][AppLanguage.English],
              replyMarkup: BuildLanguageInlineKeyboard(),
              cancellationToken: token);
            return;
        }

        var lang = GetLang(user);

        // 1) глобальные команды
        if (text.Equals("/language", StringComparison.OrdinalIgnoreCase))
        {
            await bot.SendTextMessageAsync(
                chatId,
                I18n.Text["choose_language"][lang],
                replyMarkup: BuildLanguageInlineKeyboard(),
                cancellationToken: token);
            return;
        }

        if (text.Equals("/start", StringComparison.OrdinalIgnoreCase) && user.RegistrationStep == RegistrationStep.Completed)
        {
            await bot.SendTextMessageAsync(
                chatId,
                I18n.Text["select_currency"][lang],
                replyMarkup: BuildCurrencyInlineKeyboard(),
                cancellationToken: token);

            user.RegistrationStep = RegistrationStep.WaitingCurrency;
            _userRepository.Update(user);
            await _userRepository.SaveAsync();
            return;
        }

        if (text.Equals("/start", StringComparison.OrdinalIgnoreCase) && user.RegistrationStep != RegistrationStep.Completed)
        {
            bool flowControl = await FlowControl(bot, chatId, user, lang, token);
            if (!flowControl)
            {
                return;
            }
        }

        // 2) поддержка текста-триггера на случай, если пользователь напишет руками
        if (EqualsAny(text, "Create Payment Link", "Создать платёжную ссылку", "צור קישור תשלום"))
        {
            await bot.SendTextMessageAsync(
                chatId,
                I18n.Text["select_currency"][lang],
                replyMarkup: BuildCurrencyInlineKeyboard(),
                cancellationToken: token);

            user.RegistrationStep = RegistrationStep.WaitingCurrency;
            _userRepository.Update(user);
            await _userRepository.SaveAsync();
            return;
        }

        if (EqualsAny(text, "Common Payment Issues", "Проблемы с оплатой", "בעיות נפוצות"))
        {
            var sb = new StringBuilder();
            sb.AppendLine(I18n.Text["issues_title"][lang]);
            sb.AppendLine(I18n.Text["issues_identity"][lang]);
            sb.AppendLine(I18n.Text["issues_environment"][lang]);
            sb.AppendLine(I18n.Text["issues_cards"][lang]);
            sb.AppendLine(I18n.Text["issues_other"][lang]);

            await bot.SendTextMessageAsync(chatId, sb.ToString(), cancellationToken: token);
            return;
        }

        // 3) пошаговая регистрация по текстам
        switch (user.RegistrationStep)
        {
            case RegistrationStep.WaitingLanguage:
                {
                    var chosen = ParseLanguage(text);
                    if (chosen is null)
                    {
                        // непонятный ввод — повторяем выбор языка
                        await bot.SendTextMessageAsync(
                            chatId,
                            I18n.Text["choose_language"][GetLang(user)],
                            replyMarkup: BuildLanguageInlineKeyboard(),
                            cancellationToken: token);
                        return;
                    }

                    user.Language = chosen.Value;
                    user.RegistrationStep = RegistrationStep.WaitingEmail;
                    _userRepository.Update(user);
                    await _userRepository.SaveAsync();


                    await bot.SendTextMessageAsync(chatId, I18n.Text["ask_email_with_note"][AppLanguage.English], cancellationToken: token);
                    return;
                }

            case RegistrationStep.WaitingEmail:
                {
                    if (!IsValidEmail(text))
                    {
                        await bot.SendTextMessageAsync(chatId, I18n.Text["invalid_email"][lang], cancellationToken: token);
                        return;
                    }

                    var exisintgUser = await _userRepository.GetFirstOrDefaultAsync(x => x.Name != null && x.Name.ToLower() == text.ToLower() || x.Email == text);

                    if (exisintgUser != null)
                    {
                        var tgs = await _telegramUserService.GetAllByUserIdAsync(user.Id, token);

                        // exisintgUser.TelegramId = user.TelegramId;
                        exisintgUser.Language = user.Language;
                        exisintgUser.RegistrationStep = RegistrationStep.Completed;
                        _userRepository.Delete(user);
                        _userRepository.Update(exisintgUser);
                        await _userRepository.SaveAsync();

                        foreach (var tg in tgs)
                        {
                            await _telegramUserService.RemoveAsync(tg.TelegramId, user.Id, token);
                            await _telegramUserService.AddAsync(tg.TelegramId, exisintgUser.Id, token);
                        }

                        var summary = I18n.Text["welcome_back"][lang]
                            .Replace("[User's Name]", exisintgUser.Email);

                        await bot.SendTextMessageAsync(chatId, summary, cancellationToken: token);
                        await bot.SendTextMessageAsync(chatId, I18n.Text["you_can_now_create"][lang], replyMarkup: BuildCreateLinkInlineKeyboard(), cancellationToken: token);

                        return;
                    }

                    user.Name = text;
                    user.Email = text;
                    user.RegistrationStep = RegistrationStep.WaitingPassword;
                    _userRepository.Update(user);
                    await _userRepository.SaveAsync();

                    await bot.SendTextMessageAsync(chatId, I18n.Text["ask_password"][lang], cancellationToken: token);
                    return;
                }
            case RegistrationStep.WaitingPassword:
                {
                    if (!_userService.IsValidPassword(text))
                    {
                        await bot.SendTextMessageAsync(chatId, I18n.Text["invalid_password"][lang], cancellationToken: token);
                        return;
                    }

                    await _userService.ChangePasswordAsync(user.Id, string.Empty, text, false);
                    user.RegistrationStep = RegistrationStep.WaitingPhone;
                    _userRepository.Update(user);
                    await _userRepository.SaveAsync();

                    await bot.SendTextMessageAsync(chatId, I18n.Text["ask_phone"][lang], cancellationToken: token);
                    return;
                }
            case RegistrationStep.WaitingPhone:
                {
                    if (!IsValidPhone(text))
                    {
                        await bot.SendTextMessageAsync(chatId, I18n.Text["invalid_phone"][lang], cancellationToken: token);
                        return;
                    }

                    user.Phone = text;
                    user.RegistrationStep = RegistrationStep.WaitingWallet;
                    _userRepository.Update(user);
                    await _userRepository.SaveAsync();

                    await bot.SendTextMessageAsync(chatId, I18n.Text["ask_wallet"][lang], cancellationToken: token);
                    return;
                }

            case RegistrationStep.WaitingWallet:
                {
                    if (!IsValidWallet(text))
                    {
                        await bot.SendTextMessageAsync(chatId, I18n.Text["invalid_wallet"][lang], cancellationToken: token);
                        return;
                    }

                    user.UsdcWallet = text;
                    user.RegistrationStep = RegistrationStep.Completed;
                    _userRepository.Update(user);
                    await _userRepository.SaveAsync();

                    // 1) поздравление
                    await bot.SendTextMessageAsync(chatId, I18n.Text["account_created"][lang], cancellationToken: token);

                    // 2) подробный summary
                    var username = string.IsNullOrWhiteSpace(user.Name) ? $"tg_{chatId}" : user.Name;
                    var summary = I18n.Text["account_summary"][lang]
                        .Replace("[Telegram Username]", username)
                        .Replace("[email]", user.Email ?? string.Empty)
                        .Replace("[phone]", user.Phone ?? string.Empty)
                        .Replace("[wallet address]", user.UsdcWallet ?? string.Empty);

                    await bot.SendTextMessageAsync(chatId, summary, cancellationToken: token);

                    // 3) подсказка + inline-кнопка Create Payment Link
                    await bot.SendTextMessageAsync(
                        chatId,
                        I18n.Text["you_can_now_create"][lang],
                        replyMarkup: BuildCreateLinkInlineKeyboard(),
                        cancellationToken: token);

                    // остаёмся в Completed — дальше ждём нажатия inline-кнопки (или /start)
                    return;
                }
        }

        // 4) выбор валюты и суммы (если вдруг пришёл текст, а не callback)
        if (user.RegistrationStep == RegistrationStep.WaitingCurrency)
        {
            var up = text.ToUpperInvariant();
            if (IsSupportedFiat(up))
            {
                user.Country = up;
                user.RegistrationStep = RegistrationStep.WaitingAmount;
                _userRepository.Update(user);
                await _userRepository.SaveAsync();

                await bot.SendTextMessageAsync(chatId, I18n.Text["enter_amount"][lang], cancellationToken: token);
                return;
            }

            // повторно показать клавиатуру валют
            await bot.SendTextMessageAsync(
                chatId,
                I18n.Text["select_currency"][lang],
                replyMarkup: BuildCurrencyInlineKeyboard(),
                cancellationToken: token);
            return;
        }

        if (user.RegistrationStep == RegistrationStep.WaitingAmount)
        {
            if (!TryParseAmount(text, out var amount) || amount < 50m || amount > 15000m)
            {
                await bot.SendTextMessageAsync(chatId, I18n.Text["invalid_amount"][lang], cancellationToken: token);
                return;
            }

            await SendLinksAndNextSteps(bot, chatId, user, amount, lang, token);
            return;
        }

        // fallback: покажем выбор языка inline
        await bot.SendTextMessageAsync(
            chatId,
            I18n.Text["choose_language"][lang],
            replyMarkup: BuildLanguageInlineKeyboard(),
            cancellationToken: token);
    }

    private async Task<bool> FlowControl(IBotClient bot, long chatId, UserDb user, AppLanguage lang, CancellationToken token)
    {
        switch (user.RegistrationStep)
        {
            case RegistrationStep.WaitingLanguage:
                {
                    await bot.SendTextMessageAsync(
                              chatId,
                              I18n.Text["choose_language"][GetLang(user)],
                              replyMarkup: BuildLanguageInlineKeyboard(),
                              cancellationToken: token);


                    return false;
                }

            case RegistrationStep.WaitingEmail:
                {
                    await bot.SendTextMessageAsync(chatId, I18n.Text["ask_email_with_note"][lang], cancellationToken: token);
                    return false;
                }
            case RegistrationStep.WaitingPassword:
                {
                    await bot.SendTextMessageAsync(chatId, I18n.Text["ask_password"][lang], cancellationToken: token);
                    return false;


                }
            case RegistrationStep.WaitingPhone:
                {
                    await bot.SendTextMessageAsync(chatId, I18n.Text["ask_phone"][lang], cancellationToken: token);
                    return false;
                }

            case RegistrationStep.WaitingWallet:
                {
                    await bot.SendTextMessageAsync(chatId, I18n.Text["ask_wallet"][lang], cancellationToken: token);
                    return false;
                }
        }

        return true;
    }

    // ========= ENTRY: callback queries (inline buttons) =========
    public async Task HandleCallbackAsync(IBotClient bot, long chatId, string callbackData, CancellationToken token)
    {
        var telegramUser = await _telegramUserService
            .GetByTelegramIdOrDefaultAsync(chatId, token);

        if (telegramUser is null)
                return;

        var user = await _userRepository
            .GetByIdAsync(telegramUser.UserId);

        if (user is null)
                return;

        var lang = GetLang(user);
        var data = callbackData ?? string.Empty;

        // язык
        if (data.StartsWith("lang:", StringComparison.Ordinal))
        {
            var code = data.Substring("lang:".Length);
            switch (code)
            {
                case "EN": user.Language = AppLanguage.English; break;
                case "RU": user.Language = AppLanguage.Russian; break;
                case "HE": user.Language = AppLanguage.Hebrew; break;
                case "PT": user.Language = AppLanguage.Portuguese; break;
            }
            _userRepository.Update(user);
            await _userRepository.SaveAsync();

            // после выбора языка — если пользователь ещё не зарегистрирован, продолжаем регистрацию,
            // иначе показываем главное действие: создать ссылку
            var l = GetLang(user);
            if (user.RegistrationStep == RegistrationStep.WaitingLanguage)
            {
                user.RegistrationStep = RegistrationStep.WaitingEmail;
                await bot.SendTextMessageAsync(chatId, I18n.Text["ask_email_with_note"][l], cancellationToken: token);
                _userRepository.Update(user);
                await _userRepository.SaveAsync();
                return;
            }

            bool flowControl = await FlowControl(bot, chatId, user, l, token);
            if (!flowControl)
            {
                return;
            }
            else
                await bot.SendTextMessageAsync(chatId, I18n.Text["you_can_now_create"][l], replyMarkup: BuildCreateLinkInlineKeyboard(), cancellationToken: token);

            return;
        }

        // кнопка Create Payment Link
        if (data == "action:create_link")
        {
            await bot.SendTextMessageAsync(
                chatId,
                I18n.Text["select_currency"][lang],
                replyMarkup: BuildCurrencyInlineKeyboard(),
                cancellationToken: token);

            user.RegistrationStep = RegistrationStep.WaitingCurrency;
            _userRepository.Update(user);
            await _userRepository.SaveAsync();
            return;
        }

        // выбор валюты
        if (data.StartsWith("cur:", StringComparison.Ordinal))
        {
            var cur = data.Substring("cur:".Length).ToUpperInvariant();
            if (IsSupportedFiat(cur))
            {
                user.Country = cur;
                user.RegistrationStep = RegistrationStep.WaitingAmount;
                _userRepository.Update(user);
                await _userRepository.SaveAsync();

                await bot.SendTextMessageAsync(chatId, I18n.Text["enter_amount"][lang], cancellationToken: token);
            }
            else
            {
                await bot.SendTextMessageAsync(
                    chatId,
                    I18n.Text["select_currency"][lang],
                    replyMarkup: BuildCurrencyInlineKeyboard(),
                    cancellationToken: token);
            }
            return;
        }

        // common issues (если решишь повесить кнопку)
        if (data == "action:issues")
        {
            var sb = new StringBuilder();
            sb.AppendLine(I18n.Text["issues_title"][lang]);
            sb.AppendLine(I18n.Text["issues_identity"][lang]);
            sb.AppendLine(I18n.Text["issues_environment"][lang]);
            sb.AppendLine(I18n.Text["issues_cards"][lang]);
            sb.AppendLine(I18n.Text["issues_other"][lang]);

            await bot.SendTextMessageAsync(chatId, sb.ToString(), cancellationToken: token);
            return;
        }
    }

    // ========= LINKS + NEXT STEPS =========
    private async Task SendLinksAndNextSteps(IBotClient bot, long chatId, UserDb user, decimal amount, AppLanguage lang, CancellationToken token)
    {
        try
        {
            var result = await _orchestrator.GenerateAsync(new LinkGenerationInput
            {
                RequestorUserId = user.Id,
                TargetUserId = user.Id,
                FiatCurrency = user.Country ?? "USD",
                Amount = amount,
                Email = user.Email
            }, token);

            var sb = new StringBuilder();
            sb.AppendLine(I18n.Text["links_intro"][lang]);

            var amountFormatted = FormatAmount(user.Country ?? "USD", amount);

            // поддержим оба варианта i18n: со статическим образцом "$250.50" или с плейсхолдером "{amount}"
            var amountLine = I18n.Text["amount_equivalent_line"][lang]
                .Replace("$250.50", amountFormatted)   // если в тексте из docx оставлен пример
                .Replace("{amount}", amountFormatted); // если используешь плейсхолдер

            sb.AppendLine(amountLine);
            sb.AppendLine();

            var lines = result.PaymentLinks ?? [];
            // Tier 1
            // sb.AppendLine(I18n.Text["tier1"][lang]);
            //removed paypis
            // sb.AppendLine("-");
            //if (lines.Count > 0) sb.AppendLine($"{lines[0]}");
            // Tier 2
            // sb.AppendLine(I18n.Text["tier2"][lang]);
            
            for (int i = 0; i < lines.Count; i++)
            {
                sb.AppendLine($"{i+1}.");
                sb.AppendLine($"{lines[i]}");
                sb.AppendLine();
            }

            // инструкции
            sb.AppendLine(I18n.Text["whats_next_title"][lang]);
            sb.AppendLine(I18n.Text["whats_next_copy"][lang]);
            sb.AppendLine(I18n.Text["whats_next_try_next"][lang]);
            sb.AppendLine(I18n.Text["whats_next_notify"][lang]);
            sb.AppendLine(I18n.Text["whats_next_settlement"][lang]);
            sb.AppendLine(I18n.Text["open_in_browser_warning"][lang]);

            // панель ссылок (как на скрине): две url-кнопки + действия
            var kb = BuildLinksPanel(
                lines.ElementAtOrDefault(0) is string l1 ? ExtractUrl(l1) : null,
                lines.ElementAtOrDefault(1) is string l2 ? ExtractUrl(l2) : null);

            await bot.SendTextMessageAsync(chatId, sb.ToString(), replyMarkup: kb, cancellationToken: token);

            user.RegistrationStep = RegistrationStep.Completed;
            _userRepository.Update(user);
            await _userRepository.SaveAsync();
        }
        catch (Exception ex)
        {
            await bot.SendTextMessageAsync(chatId,
                I18n.Text["links_error"][lang].Replace("{error}", ex.Message),
                cancellationToken: token);
        }
    }

    // ========= INLINE KEYBOARDS =========

    private static InlineKeyboardMarkup BuildCreateLinkInlineKeyboard()
        => new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🧾 Create Payment Link", "action:create_link")
            }
        });

    private static InlineKeyboardMarkup BuildLanguageInlineKeyboard()
        => new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("English", "lang:EN"),
                InlineKeyboardButton.WithCallbackData("Русский", "lang:RU"),
                InlineKeyboardButton.WithCallbackData("עברית",  "lang:HE"),
                InlineKeyboardButton.WithCallbackData("Portugues", "lang:PT")
            }
        });

    private static readonly string[] SupportedFiat = new[]
    {
        //"EUR", "USD"
        "AED","ARS","AUD","BGN","BRL","CAD","CHF","CZK","DKK","EUR",
        "GBP","HKD","HUF","ILS","JPY","KRW","KWD","MXN","NOK","NZD",
        "PLN","QAR","RON","SAR","SEK","SGD","TRY","USD","ZAR"
    };

    private static bool IsSupportedFiat(string c)
        => Array.IndexOf(SupportedFiat, c) >= 0;

    private static InlineKeyboardMarkup BuildCurrencyInlineKeyboard(int columns = 4)
    {
        var codes = SupportedFiat.OrderBy(c => c, StringComparer.Ordinal).ToArray();
        var rows = new List<InlineKeyboardButton[]>();
        for (int i = 0; i < codes.Length; i += columns)
        {
            rows.Add(codes
                .Skip(i)
                .Take(columns)
                .Select(c => InlineKeyboardButton.WithCallbackData(c, $"cur:{c}"))
                .ToArray());
        }
        return new InlineKeyboardMarkup(rows);
    }

    private static InlineKeyboardMarkup BuildLinksPanel(string? link1, string? link2)
    {
        var rows = new List<InlineKeyboardButton[]>();

        // ряд с URL-кнопками (если ссылки есть)
        var urlRow = new List<InlineKeyboardButton>();
        //if (!string.IsNullOrWhiteSpace(link1))
        //    urlRow.Add(InlineKeyboardButton.WithUrl("🔗 Link 1", link1!));
        //if (!string.IsNullOrWhiteSpace(link2))
        //    urlRow.Add(InlineKeyboardButton.WithUrl("🔗 Link 2 - Backup", link2!));
        if (urlRow.Count > 0) rows.Add(urlRow.ToArray());

        // ряд с общими действиями
        rows.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("⚠️ Common Rejection Reasons", "action:issues"),
        });

        // ряд с навигацией
        rows.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("🧾 Create Payment Link", "action:create_link"),
            InlineKeyboardButton.WithCallbackData("🏠 Main Menu", "action:main")
        });

        return new InlineKeyboardMarkup(rows);
    }

    // ========= helpers =========

    private static bool EqualsAny(string input, params string[] variants)
        => variants.Any(v => string.Equals(input, v, StringComparison.OrdinalIgnoreCase));

    private static bool TryParseAmount(string s, out decimal value)
        => decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out value)
           || decimal.TryParse(s, NumberStyles.Number, CultureInfo.GetCultureInfo("ru-RU"), out value)
           || decimal.TryParse(s, NumberStyles.Number, CultureInfo.GetCultureInfo("he-IL"), out value);

    private bool IsValidEmail(string email) 
        => Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");



    private async Task<bool> IsUserExist(string email)
    {
        var exisintgUser = await _userRepository.GetFirstOrDefaultAsync(x => x.Name == email || x.Email == email);

        return exisintgUser != null;
    }

    private static bool IsValidPhone(string phone)
        => Regex.IsMatch(phone, @"^\+?[1-9]\d{1,14}$");

    private static bool IsValidWallet(string wallet)
        => !string.IsNullOrWhiteSpace(wallet) && wallet.StartsWith("0x") && wallet.Length == 42;

    private static string ExtractUrl(string line)
    {
        // из "Tier1-Link-1 https://example.com/l1" достаём сам URL
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.LastOrDefault() ?? line;
    }

    private static AppLanguage GetLang(UserDb user)
        => user.Language is AppLanguage l ? l : AppLanguage.English;

    private static string FormatAmount(string fiat, decimal amount)
    {
        // символы для самых частых валют
        var symbol = fiat switch
        {
            "USD" => "$",
            "EUR" => "€",
            "GBP" => "£",
            "ILS" => "₪",
            _ => ""   // для прочих оставим код после числа
        };

        // без лишних нулей: 50 -> "50", 50.5 -> "50.5", 50.75 -> "50.75"
        var num = amount % 1m == 0m
            ? amount.ToString("0", CultureInfo.InvariantCulture)
            : amount.ToString("0.##", CultureInfo.InvariantCulture);

        // "€50", "$50.5", "₪50" или "50 ABC" если символа нет
        return symbol != "" ? $"{symbol}{num}" : $"{num} {fiat}";
    }

    private static AppLanguage? ParseLanguage(string input)
    {
        var t = (input ?? "").Trim();
        return t switch
        {
            "English" or "EN" => AppLanguage.English,
            "Русский" or "RU" => AppLanguage.Russian,
            "עברית" or "HE" => AppLanguage.Hebrew,
            "Portugues" or "PT" => AppLanguage.Portuguese,
        _ => (AppLanguage?)null
        };
    }
}