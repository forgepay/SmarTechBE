using CryptoOnRamp.BLL.Interfaces;
using MicPic.Infrastructure.Extensions;
using MicPic.Infrastructure.RateLimit.Attributes;
using MicPic.Infrastructure.RateLimit.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoOnRamp.API.Controllers;

[Route("api/telegram-users")]
[ApiController]
[Authorize]
[AppRateLimitPolicy(AppRateLimitPolicyName.PerUser)]
[AppRateLimitPolicy(AppRateLimitPolicyName.Default, Priority = int.MinValue)]
public class TelegramUsersController(ITelegramUserService telegramUserService) : ControllerBase
{
    private readonly ITelegramUserService _telegramUserService = telegramUserService;

    [HttpGet]
    public async Task<ActionResult<List<long>>> ListAsync(CancellationToken cancellationToken)
    {
        var telegramUsers = await _telegramUserService
            .GetAllByUserIdAsync(User.Id(), cancellationToken);

        var telegramIds = telegramUsers
            .Select(tu => tu.TelegramId)
            .ToList();

        return Ok(telegramIds);
    }

    [HttpPost("{telegramId:long}")]
    public async Task<IActionResult> AddAsync(long telegramId, CancellationToken cancellationToken)
    {
        await _telegramUserService
            .AddAsync(telegramId, User.Id(), cancellationToken);

        return Ok();
    }        

    [HttpDelete("{telegramId:long}")]
    public async Task<IActionResult> RemoveAsync(long telegramId, CancellationToken cancellationToken)
    {
        await _telegramUserService
            .RemoveAsync(telegramId, User.Id(), cancellationToken);

        return Ok();
    }        
}
