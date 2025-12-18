using System.Text;
using CryptoOnRamp.BLL.Services.Transak.Internal;
using MicPic.Infrastructure.Exceptions;
using MicPic.Infrastructure.RateLimit.Attributes;
using MicPic.Infrastructure.RateLimit.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CryptoOnRamp.API.Controllers.Webhooks;

[Route("api/webhook")]
[ApiController]
[AppRateLimitPolicy(AppRateLimitPolicyName.PerIp)]
[AppRateLimitPolicy(AppRateLimitPolicyName.Default, Priority = int.MinValue)]
public partial class TransakWebhookController(ITransakWebhookService transakWebhookService, ILogger<TransakWebhookController> logger) : ControllerBase
{
    private readonly ITransakWebhookService _transakWebhookService = transakWebhookService;


    [HttpPost("transak")]
    public async Task<IActionResult> Handle(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);

        var body = await reader.ReadToEndAsync(cancellationToken);
        var signature = Request.Headers["X-Request-Signature"].ToString();

        LogRequest(signature, body);

        if (string.IsNullOrWhiteSpace(signature))
            throw new AppException("Missing signature", BusinessErrorCodes.Unauthorized);

        if (!_transakWebhookService.ValidateSignature(body, signature))
            throw new AppException("Invalid signature", BusinessErrorCodes.Unauthorized);

        await _transakWebhookService.ProcessAsync(body, cancellationToken);

        return Ok();
    }


    #region Logging

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Transak webhook request. Signature: '{signature}'. Body: '{body}'.")]
    private partial void LogRequest(string signature, string body);

    #endregion        
}
