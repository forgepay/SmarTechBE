namespace CryptoOnRamp.BLL.Services.Transak.Internal;

public interface ITransakWebhookService
{
    Task ProcessAsync(string body, CancellationToken cancellationToken);
    bool ValidateSignature(string body, string signature);
}
