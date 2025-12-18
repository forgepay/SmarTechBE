using CryptoOnRamp.BLL.Interfaces;
using MicPic.Infrastructure.RateLimit.Attributes;
using MicPic.Infrastructure.RateLimit.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace CryptoOnRamp.API.Controllers.Webhooks;

[Route("api/webhook")]
[ApiController]
[AppRateLimitPolicy(AppRateLimitPolicyName.PerIp)]
[AppRateLimitPolicy(AppRateLimitPolicyName.Default, Priority = int.MinValue)]
public class OnramperWebhookController(IOnramperWebhookService service, ILogger<OnramperWebhookController> logger) : ControllerBase
{
    private readonly IOnramperWebhookService _service = service;
    private readonly ILogger<OnramperWebhookController> _logger = logger;

    [HttpPost("onramper")]
    public async Task<IActionResult> HandleAsync()
    {
        var correlationId = Guid.NewGuid().ToString("N");

        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync();
        Request.Body.Position = 0;

        var signature = Request.Headers["X-Onramper-Webhook-Signature"].ToString();

        // log metadata
        _logger.LogInformation(
            "📥 [{CorrelationId}] Received webhook: Signature={Signature}, PayloadLength={Length} bytes",
            correlationId,
            string.IsNullOrEmpty(signature) ? "❌ empty" : $"✅ provided",
            rawBody.Length);

        // log raw body
        _logger.LogInformation("📦 [{CorrelationId}] Raw Onramper webhook signature: {signature}", correlationId, signature);
        _logger.LogInformation("📦 [{CorrelationId}] Raw Onramper webhook body: {RawBody}", correlationId, rawBody);

        if (!_service.ValidateSignature(rawBody, signature))
        {
            _logger.LogWarning("⛔ [{CorrelationId}] Invalid Onramper webhook signature", correlationId);
            return Unauthorized(new { error = "Invalid Onramper webhook signature", correlationId });
        }

        try
        {
            await _service.ProcessAsync(rawBody, correlationId);

            _logger.LogInformation("✅ [{CorrelationId}] Webhook processed successfully at {TimeUtc}",
                correlationId, DateTime.UtcNow);

            return Ok(new { status = "ok", correlationId });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("⛔ [{CorrelationId}] Unauthorized webhook attempt. Error={Error}",
                correlationId, ex.Message);
            return Unauthorized(new { error = ex.Message, correlationId });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("🔎 [{CorrelationId}] Resource not found while processing webhook. Error={Error}",
                correlationId, ex.Message);
            return NotFound(new { error = ex.Message, correlationId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 [{CorrelationId}] Unexpected error while processing webhook", correlationId);
            return StatusCode(500, new { error = "Webhook processing error", correlationId });
        }
    }
}
