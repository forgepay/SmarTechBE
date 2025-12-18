using CryptoOnRamp.BLL.Services.LinkGenerator.Models;

namespace CryptoOnRamp.BLL.Services.LinkGenerator;

public interface ILinkSource
{
    IAsyncEnumerable<LinkResponse> GenerateLinksAsync(LinkRequest request, CancellationToken cancellationToken);
}