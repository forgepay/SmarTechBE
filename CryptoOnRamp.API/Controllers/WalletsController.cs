using CryptoOnRamp.BLL.Interfaces;
using MicPic.Infrastructure.RateLimit.Attributes;
using MicPic.Infrastructure.RateLimit.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoOnRamp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[AppRateLimitPolicy(AppRateLimitPolicyName.PerUser)]
[AppRateLimitPolicy(AppRateLimitPolicyName.Default, Priority = int.MinValue)]
public class WalletsController(IWalletService walletService) : ControllerBase
{
    private readonly IWalletService _walletService = walletService;

    [HttpGet("{address}/balance")]
    [Authorize(Policy = "AdminOrSuperAgent")]
    public async Task<IActionResult> GetBalance([FromRoute]string address)
    {
        return Ok( await _walletService.GetBalanceAsync(address));
    }
}
