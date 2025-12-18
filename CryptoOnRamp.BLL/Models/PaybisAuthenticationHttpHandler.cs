using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace CryptoOnRamp.BLL.Models;

internal class PaybisAuthenticationHttpHandler : DelegatingHandler
{
    private readonly IOptionsMonitor<PaybisOptions> _optionsMonitor;

    public PaybisAuthenticationHttpHandler(IOptionsMonitor<PaybisOptions> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _optionsMonitor.CurrentValue.ApiKey);
        return base.SendAsync(request, cancellationToken);
    }
}
