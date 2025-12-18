using CryptoOnRamp.API.Models;
using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using MicPic.Infrastructure.RateLimit.Attributes;
using MicPic.Infrastructure.RateLimit.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoOnRamp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[AppRateLimitPolicy(AppRateLimitPolicyName.PerUser)]
[AppRateLimitPolicy(AppRateLimitPolicyName.Default, Priority = int.MinValue)]
public class UsersController(IUserService userService) : ControllerBase
{
    private IUserService _userService = userService;

    [HttpPost]
    [Authorize(Policy = "AdminOrSuperAgent")]
    public async Task<ActionResult> Create(CreateUserReuqest createUser)
    {
        var user = await _userService.CreateAsync(createUser);
        return Ok(user);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await _userService
            .DeleteAsync(id, cancellationToken);
        
        return Ok();
    }

    [HttpGet]
    [Authorize(Policy = "AdminOrSuperAgent")]
    public async Task<IActionResult> GetUsers([FromQuery] UserRole? role, [FromQuery] int? parentId)
    {
        var users = await _userService.GetUsersAsync(role, parentId);
        return Ok(users);
    }

    [HttpPut("{id:int}/address")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> UpdateWalletAddress([FromRoute] int id, [FromBody] UpdateWalletRequest request)
    {
        await _userService.UpdateUserWalletAsync(id, request.NewWalletAddress);

        return Ok(new
        {
            message = "Wallet address updated successfully.",
            userId = id,
            newWallet = request.NewWalletAddress
        });
    }
}
