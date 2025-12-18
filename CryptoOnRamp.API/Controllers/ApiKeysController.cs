using CryptoOnRamp.BLL.Interfaces;
using MicPic.Infrastructure.Extensions;
using MicPic.Infrastructure.RateLimit.Attributes;
using MicPic.Infrastructure.RateLimit.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoOnRamp.API.Controllers;

[Route("api/api-keys")]
[ApiController]
[Authorize]
[AppRateLimitPolicy(AppRateLimitPolicyName.PerUser)]
[AppRateLimitPolicy(AppRateLimitPolicyName.Default, Priority = int.MinValue)]
public class ApiKeysController(IApiKeyService apiKeyService) : ControllerBase
{
    private readonly IApiKeyService _apiKeyService = apiKeyService;

    /// <summary>
    /// Retrieves Api key name in the format XXXX*****.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetNameAsync(CancellationToken cancellationToken)
    {
        var name = await _apiKeyService
            .GetNameAsync(User.Id(), cancellationToken);

        return Ok(new { name });
    }

    /// <summary>
    /// Generate new API key instead of the old one.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GenerateAsync(CancellationToken cancellationToken)
    {
        var token = await _apiKeyService
            .GenerateAsync(User.Id(), cancellationToken);

        return Ok(new { token });
    }

    /// <summary>
    /// Remove existing API key.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> RemoveAsync(CancellationToken cancellationToken)
    {
        await _apiKeyService
            .RemoveAsync(User.Id(), cancellationToken);

        return Ok();
    }
}
