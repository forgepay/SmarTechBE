namespace CryptoOnRamp.BLL.Interfaces;

public interface IWebHookService
{
    Task<string?> GetWebhookEndpointAsync(int userId, CancellationToken cancellationToken);
    Task RemoveWebhookEndpointAsync(int userId, CancellationToken cancellationToken);
    Task UpdateWebhookEndpointAsync(int userId, Uri uri, CancellationToken cancellationToken);
    Task<string> GenerateWebhookAuthorizationTokenAsync(int userId, CancellationToken cancellationToken);
    Task<bool> CallWebhookEndpointAsync<TPayload>(int userId, string action, TPayload payload, CancellationToken cancellationToken);
}
