using CryptoOnRamp.BLL.Interfaces;
using MicPic.Infrastructure.Extensions;
using MicPic.Infrastructure.RateLimit.Attributes;
using MicPic.Infrastructure.RateLimit.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoOnRamp.API.Controllers;

[Route("api/webhooks")]
[ApiController]
[Authorize]
[AppRateLimitPolicy(AppRateLimitPolicyName.PerUser)]
[AppRateLimitPolicy(AppRateLimitPolicyName.Default, Priority = int.MinValue)]
public class WebhooksController(IWebHookService webHookService) : ControllerBase
{
    private readonly IWebHookService _webHookService = webHookService;

    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var endpoint = await _webHookService
            .GetWebhookEndpointAsync(User.Id(), cancellationToken);

        return Ok(new { endpoint });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAsync([FromBody] Uri uri, CancellationToken cancellationToken)
    {
        await _webHookService
            .UpdateWebhookEndpointAsync(User.Id(), uri, cancellationToken);

        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> RemoveAsync(CancellationToken cancellationToken)
    {
        await _webHookService
            .RemoveWebhookEndpointAsync(User.Id(), cancellationToken);

        return Ok();
    }

    [HttpPost("generate-token")]
    public async Task<IActionResult> GenerateTokenAsync(CancellationToken cancellationToken)
    {
        var token = await _webHookService
            .GenerateWebhookAuthorizationTokenAsync(User.Id(), cancellationToken);

        return Ok(new { token });
    }        
}
