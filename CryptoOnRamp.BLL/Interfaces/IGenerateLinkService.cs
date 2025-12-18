using CryptoOnRamp.BLL.Models;

namespace CryptoOnRamp.BLL.Interfaces;

public interface IGenerateLinkService
{
    Task<GenerateLinkResponse> GenerateAsync(GenerateLinkRequest req, CancellationToken ct);
}
