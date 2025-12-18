using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using MicPic.Infrastructure.RateLimit.Attributes;
using MicPic.Infrastructure.RateLimit.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoOnRamp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AppRateLimitPolicy(AppRateLimitPolicyName.PerUser)]
[AppRateLimitPolicy(AppRateLimitPolicyName.Default, Priority = int.MinValue)]
public class StatsController(IStatsService stats) : ControllerBase
{
    private readonly IStatsService _stats = stats;

    [HttpGet("global")]
    [Authorize]
    public async Task<ActionResult<GlobalStatsDto>> GetGlobal()
        => Ok(await _stats.GetGlobalAsync());


    [HttpGet("user/{id:int}")]
    [Authorize(Policy = "AdminOrSuperAgent")]
    public async Task<ActionResult<UserStatsDto>> GetUser(int id)
        => Ok(await _stats.GetUserAsync(id));
}
