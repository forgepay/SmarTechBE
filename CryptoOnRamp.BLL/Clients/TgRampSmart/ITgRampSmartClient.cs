using CryptoOnRamp.BLL.Clients.TgRampSmart.Models;

namespace CryptoOnRamp.BLL.Clients.TgRampSmart.Internal;

public interface ITgRampSmartClient
{
    Task<ApiKeyDto> GenerateApiKeyAsync(CancellationToken cancellationToken);
    Task<PayoutDto> CreatePayoutAsync(string apiKey, PayoutRequestDto request, CancellationToken cancellationToken);
    Task<PayoutDto> ClaimPayoutAsync(string apiKey, Guid id, CancellationToken cancellationToken);
}
