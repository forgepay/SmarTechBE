namespace CryptoOnRamp.BLL.Interfaces;

public interface IPaybisWebhookService
{
    Task ProcessAsync(string rawBody, string correlationId);
    bool ValidateSignature(string rawBody, string signatureBase64);
}
