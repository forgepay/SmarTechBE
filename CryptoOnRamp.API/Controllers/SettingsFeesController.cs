using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using MicPic.Infrastructure.RateLimit.Attributes;
using MicPic.Infrastructure.RateLimit.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoOnRamp.API.Controllers;

[ApiController]
[Route("api/settings")]
[AppRateLimitPolicy(AppRateLimitPolicyName.PerUser)]
[AppRateLimitPolicy(AppRateLimitPolicyName.Default, Priority = int.MinValue)]
public class SettingsFeesController(IFeeService fees) : ControllerBase
{
    private readonly IFeeService _fees = fees;

    // GET /settings/fees
    [HttpGet("fees")]
    [Authorize(Policy = "AdminOrSuperAgent")]
    public async Task<ActionResult<GetFeeSettingsResponse>> GetFees()
        => Ok(await _fees.GetSettingsAsync());


    [HttpPost("fees")]
    [Authorize(Policy = "AdminOrSuperAgent")]
    public async Task<ActionResult<object>> UpdateFees([FromBody] FeeUpdateRequest req)
    {
        var ok = await _fees.UpdateAsync(req);
        return Ok(new { updated = ok });
    }
}
