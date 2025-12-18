namespace CryptoOnRamp.BLL.Interfaces;

public interface IOnramperWebhookService
{
    Task ProcessAsync(string rawBody, string correlationId);
    bool ValidateSignature(string rawBody, string signature);
}
