using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CryptoOnRamp.BLL.Extensions;
using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;
using MicPic.Infrastructure.Exceptions;

namespace CryptoOnRamp.BLL.Services;

internal class WebHookService(
    IUserRepository userRepository,
    IHttpClientFactory httpClientFactory) : IWebHookService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;


    #region IWebhookService Implementation

    public async Task<string?> GetWebhookEndpointAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository
            .GetActiveUserAsync(userId, cancellationToken);

        return user.WebhookUrl;
    }

    public async Task UpdateWebhookEndpointAsync(int userId, Uri uri, CancellationToken cancellationToken)
    {
        var user = await _userRepository
            .GetActiveUserAsync(userId, cancellationToken);

        if (string.IsNullOrWhiteSpace(user.WebhookAuthorizationToken))
            throw new InvalidOperationException("User does not have a webhook authorization token.");

        await AuthorizeWebHookEndpointAsync(uri, user.WebhookAuthorizationToken, cancellationToken);

        user.WebhookUrl = uri.ToString();

        await _userRepository
            .UpdateUserAsync(user, cancellationToken);
    }

    public async Task RemoveWebhookEndpointAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository
            .GetActiveUserAsync(userId, cancellationToken);

        user.WebhookUrl = null;

        await _userRepository
            .UpdateUserAsync(user, cancellationToken);
    }

    public async Task<string> GenerateWebhookAuthorizationTokenAsync(int userId, CancellationToken cancellationToken)
    {
        var token = GenerateRandomToken();

        var user = await _userRepository
            .GetActiveUserAsync(userId, cancellationToken);

        user.WebhookAuthorizationToken = token;

        await _userRepository
            .UpdateUserAsync(user, cancellationToken);

        return token;
    }

    public async Task<bool> CallWebhookEndpointAsync<TPayload>(int userId, string action, TPayload payload, CancellationToken cancellationToken)
    {
        var user = await _userRepository
            .GetActiveUserOrDefaultAsync(userId, cancellationToken);

        if (user is null || string.IsNullOrWhiteSpace(user.WebhookUrl))
            return false;

        if (string.IsNullOrWhiteSpace(user.WebhookAuthorizationToken))
            throw new InvalidOperationException("User does not have a webhook authorization token.");

        await CallWebHookEndpointAsync(new Uri(user.WebhookUrl), new WebHookRequest<TPayload>(action, payload), user.WebhookAuthorizationToken, cancellationToken);

        return true;
    }

    #endregion


    #region Private Methods

    private async Task AuthorizeWebHookEndpointAsync(Uri uri, string token, CancellationToken cancellationToken)
    {
        try
        {
            await CallWebHookEndpointAsync(uri, new WebHookRequest<string?>("Authorization", token), token, cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode.HasValue)
        {
            // ignore not success status code
            // throw new AppException($"WebHook endpoint returns not success status code: {ex.StatusCode.Value:D}.", BusinessErrorCodes.ValidationError);
            return;
        }
        catch (HttpRequestException ex)
        {
            throw new AppException($"Failed to call WebHook endpoint.", BusinessErrorCodes.ValidationError) { TechnicalMessage = ex.Message };
        }
        catch (NotSupportedException ex)
        {
            throw new AppException($"Failed to call WebHook endpoint.", BusinessErrorCodes.ValidationError) { TechnicalMessage = ex.Message };
        }
        catch (InvalidOperationException ex)
        {
            throw new AppException($"Failed to call WebHook endpoint.", BusinessErrorCodes.ValidationError) { TechnicalMessage = ex.Message };
        }
        catch (Exception ex) when (ex is not AppException)
        {
            throw new AppException("Failed to call WebHook endpoint.", BusinessErrorCodes.ValidationError);
        }
    }

    private async Task<string> CallWebHookEndpointAsync<TPayload>(Uri uri, TPayload payload, string token, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory
            .CreateClient("WebHook");

        var body = JsonSerializer.Serialize(payload);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            RequestUri = uri,
            Method = HttpMethod.Post,
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

        httpRequest.Headers.Add("X-COR-Signature", BuildSignature(body, token));

        var httpResponse = await client
            .SendAsync(httpRequest, cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        var content = await httpResponse.Content
            .ReadAsStringAsync(cancellationToken);

        return content;
    }

    private static string BuildSignature(string payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(payloadBytes);

        return Convert.ToBase64String(hashBytes);
    }

    private static string GenerateRandomToken(int length = 32)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        var random = new Random();
        var secretChars = new char[length];

        for (int i = 0; i < length; i++)
        {
            secretChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(secretChars);
    }

    #endregion
}
