using CryptoOnRamp.BLL.Services.LinkGenerator;
using CryptoOnRamp.BLL.Services.LinkGenerator.Models;

namespace CryptoOnRamp.BLL.Services.Transak;

public interface ITransakService : ILinkSource
{
    Task<LinkResponse> GenerateLinkAsync(LinkRequest request, CancellationToken cancellationToken);
}
