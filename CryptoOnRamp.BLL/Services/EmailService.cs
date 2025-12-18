using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text.Encodings.Web;

namespace CryptoOnRamp.BLL.Services;

public class EmailService(IOptions<EmailServiceOptions> options, IOptions<AppilcationSettings> appilcationSettings) : IEmailService
{
    private readonly EmailServiceOptions _options = options.Value;
    private readonly SendGridClient _client = new(options.Value.SendGridApiKey);
    private readonly AppilcationSettings _appilcationSettings = appilcationSettings.Value;

    public async Task SendEmailAsync(string address, string subject, string bodyText, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Recipient address is required.", nameof(address));

        var from = new EmailAddress(_options.SenderAddress, _options.SenderName);
        var to = new EmailAddress(address);

        // Текст и HTML версии
        var plainText = bodyText;
        var html = $"<div style=\"font-family:Segoe UI,Arial,sans-serif;font-size:14px;line-height:1.6;\">" +
                   $"<p>{System.Net.WebUtility.HtmlEncode(bodyText).Replace("\n", "<br/>")}</p>" +
                   $"</div>";

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainText, html);

        // Можно добавить категорию/тэги
        msg.AddCategory("transactional");

        // Отправка
        var response = await _client.SendEmailAsync(msg, cancellationToken);

        if ((int)response.StatusCode >= 400)
        {
            var body = await response.Body.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"SendGrid send failed: {(int)response.StatusCode} {response.StatusCode}. Body: {body}");
        }
    }

    public async Task SendResetPasswordAsync(string? address, string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Recipient address is required.", nameof(address));

        if (string.IsNullOrWhiteSpace(_appilcationSettings.UrlUi))
            throw new InvalidOperationException("UiBaseUrl must be configured.");

        // Безопасная сборка URL с экранированием
        var url = BuildResetUrl(_appilcationSettings.UrlUi, address, token);

        var subject = "Reset Your Password";
        var body = $"Your link to reset the password:\n{url}\n\n" +
                   "This link will expire in 10 minutes.";

        // Альтернативно можно сделать более красивый HTML с кнопкой:
        var html = $@"
            <div style=""font-family:Segoe UI,Arial,sans-serif;font-size:14px;line-height:1.6;"">
              <p>Click the button below to reset your password.</p>
              <p><a href=""{url}"" style=""display:inline-block;background:#3b82f6;color:#fff;
                    padding:10px 16px;border-radius:6px;text-decoration:none;"">Reset Password</a></p>
              <p>If the button doesn't work, copy this link:</p>
              <p><a href=""{url}"">{url}</a></p>
              <p><em>This link will expire in 10 minutes.</em></p>
            </div>";

        await SendRichAsync(address, subject, plainText: body, htmlBody: html, cancellationToken);
    }

    // Вспомогательная отправка с HTML
    private async Task SendRichAsync(string address, string subject, string plainText, string htmlBody, CancellationToken cancellationToken)
    {
        var from = new EmailAddress(_options.SenderAddress, _options.SenderName);
        var to = new EmailAddress(address);

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainText, htmlBody);
        msg.AddCategory("password-reset");

        var response = await _client.SendEmailAsync(msg, cancellationToken);
        if ((int)response.StatusCode >= 400)
        {
            var body = await response.Body.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"SendGrid send failed: {(int)response.StatusCode} {response.StatusCode}. Body: {body}");
        }
    }

    private static string BuildResetUrl(string uiBase, string address, string token)
    {
        if (!uiBase.EndsWith("/")) uiBase += "/";

        var enc = UrlEncoder.Default;
        var uid = enc.Encode(address.ToString());
        var tkn = enc.Encode(token);

        return $"{uiBase}reset-password?Email={uid}&token={token}";
    }
}
