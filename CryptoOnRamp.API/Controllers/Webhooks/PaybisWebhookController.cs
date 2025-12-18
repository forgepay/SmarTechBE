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
public class PaybisWebhookController(IPaybisWebhookService service, ILogger<PaybisWebhookController> logger) : ControllerBase
{
    private readonly IPaybisWebhookService _service = service;
    private readonly ILogger<PaybisWebhookController> _logger = logger;

    [HttpPost("paybis")]
    public async Task<IActionResult> Handle()
    {
        var correlationId = Guid.NewGuid().ToString("N");

        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync();
        Request.Body.Position = 0;

        var signatureB64 = Request.Headers["X-Request-Signature"].ToString();

        // log metadata
        _logger.LogInformation(
            "📥 [{CorrelationId}] Received Paybis webhook: Signature={Signature}, PayloadLength={Length} bytes",
            correlationId,
            string.IsNullOrEmpty(signatureB64) ? "❌ empty" : "✅ provided",
            rawBody.Length);

        // log raw body
        _logger.LogInformation("📦 [{CorrelationId}] Raw Paybis webhook signature: {signature}", correlationId, signatureB64);
        _logger.LogInformation("📦 [{CorrelationId}] Raw Paybis webhook body: {RawBody}", correlationId, rawBody);

        if (!_service.ValidateSignature(rawBody, signatureB64))
        {
            _logger.LogWarning("⛔ [{CorrelationId}] Invalid Paybis webhook signature", correlationId);
            return Unauthorized(new { error = "Invalid Paybis webhook signature", correlationId });
        }

        try
        {
            await _service.ProcessAsync(rawBody, correlationId);

            _logger.LogInformation("✅ [{CorrelationId}] Paybis webhook processed successfully at {TimeUtc}",
                correlationId, DateTime.UtcNow);

            return Ok(new { status = "ok", correlationId });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("⛔ [{CorrelationId}] Unauthorized Paybis webhook attempt. Error={Error}",
                correlationId, ex.Message);
            return Unauthorized(new { error = ex.Message, correlationId });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("🔎 [{CorrelationId}] Paybis resource not found while processing webhook. Error={Error}",
                correlationId, ex.Message);
            return NotFound(new { error = ex.Message, correlationId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 [{CorrelationId}] Unexpected error while processing Paybis webhook", correlationId);
            return StatusCode(500, new { error = "Webhook processing error", correlationId });
        }
    }
}
