using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using MicPic.Infrastructure.Extensions;
using MicPic.Infrastructure.RateLimit.Attributes;
using MicPic.Infrastructure.RateLimit.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoOnRamp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[AppRateLimitPolicy(AppRateLimitPolicyName.PerIp)]
[AppRateLimitPolicy(AppRateLimitPolicyName.Default, Priority = int.MinValue)]
public class AuthController(IIdentityService identityService, IUserService userService) : ControllerBase
{
    private readonly IIdentityService _identityService = identityService;
    private readonly IUserService _userService = userService;

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] CreateSelfUserReuqest model)
    {
        await _userService
            .CreateSelfRegisteredAsync(model);

        return Ok();
    }

    [HttpPost("Login")]
    public async Task<IActionResult> LoginAsync([FromBody] GetTokenRequest request, CancellationToken cancellationToken)
    {
        var context = await _identityService
            .LoginAsync(request.Name, request.Passcode);

        return Ok(context);
    }

    [HttpPost("RefreshToken")]
    public async Task<IActionResult> RefreshTokenAsync(
     [FromBody] RefreshTokenRequest request,
     CancellationToken cancellationToken)
    {
        var context = await _identityService
            .RefreshTokenAsync(request.Name, request.RefreshToken, cancellationToken);

        return Ok(context);
    }


    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordViewModel model)
    {
        await _userService
            .ChangePasswordAsync(User.Id(), model.OldPassword, model.NewPassword);

        return Ok();
    }

    [HttpPost]
    [Route("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest model)
    {
        await _identityService
            .GeneratePasswordResetTokenAsync(model.Email);

        return Ok();
    }

    [HttpPost]
    [Route("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest model)
    {
        await _identityService
            .ResetPasswordAsync(model.Email, model.Token, model.NewPassword);

        return Ok();
    }


    [Authorize(Policy = "Admin")]
    [HttpGet("Admin")]
    public IActionResult AccessAdmin()
    {
        return Ok();
    }


    [Authorize(Policy = "SuperAgent")]
    [HttpGet("SuperAgent")]
    public IActionResult AccessSuperAgent()
    {
        return Ok();
    }


    [Authorize(Policy = "Agent")]
    [HttpGet("Agent")]
    public IActionResult AccessAgent()
    {
        return Ok();
    }
}
