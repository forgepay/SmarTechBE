namespace CryptoOnRamp.BLL.Services.Transak.Internal;

internal sealed partial class TransakWebhookService : ITransakWebhookService
{
    public bool ValidateSignature(string body, string signature)
    {
        return true;
    }

    public Task ProcessAsync(string body, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
