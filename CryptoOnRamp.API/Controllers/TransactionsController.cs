using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using CryptoOnRamp.DAL.Models;
using MicPic.Infrastructure.RateLimit.Attributes;
using MicPic.Infrastructure.RateLimit.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoOnRamp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AppRateLimitPolicy(AppRateLimitPolicyName.Default, Priority = int.MinValue)]
public class TransactionsController(
    ITransactionService transactionService) : ControllerBase
{
    private readonly ITransactionService _transactionService = transactionService;

    // GET /api/transactions
    [HttpGet]
    [Authorize]
    [AppRateLimitPolicy(AppRateLimitPolicyName.PerUser)]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int? userId,
        [FromQuery] TransactionStatus? status,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var pg = (page is >= 1) ? page.Value : 1;
        var ps = (pageSize is >= 1 and <= 10000) ? pageSize.Value : 100;

        var transactions = await _transactionService
            .GetTransactionsAsync(userId, status, dateFrom, dateTo, pg, ps);

        return Ok(transactions);
    }

    [HttpGet("{transactionId}")]
    [Authorize]
    [AppRateLimitPolicy(AppRateLimitPolicyName.PerUser)]
    public async Task<IActionResult> GetTransaction([FromRoute] int transactionId)
    {
        var transactions = await _transactionService
            .GetTransactionAsync(transactionId);

        return Ok(transactions);
    }

    [HttpGet("payouts")]
    [Authorize]
    [AppRateLimitPolicy(AppRateLimitPolicyName.PerUser)]
    public async Task<IActionResult> GetPayouts(
        [FromQuery] int? userId,
        [FromQuery] PayoutStatusDb? status,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var pg = (page is >= 1) ? page.Value : 1;
        var ps = (pageSize is >= 1 and <= 10000) ? pageSize.Value : 100;

        var payouts = await _transactionService.GetPayoutsAsync(
            userId, status, dateFrom, dateTo, pg, ps);

        return Ok(payouts);
    }

    [HttpGet("payouts/{payoutId}")]
    [Authorize]
    [AppRateLimitPolicy(AppRateLimitPolicyName.PerUser)]
    public async Task<IActionResult> GetPayouts([FromRoute] int payoutId)
    {
        var transactions = await _transactionService.GetPayoutAsync(payoutId);
        return Ok(transactions);
    }

    [HttpGet("sessions/{sessionId}")]
    [AppRateLimitPolicy(AppRateLimitPolicyName.PerIp)]
    public async Task<IActionResult> GetSessionByIdAsync([FromRoute] string sessionId)
    {
        var transactions = await _transactionService.GetSessionByIdAsync(sessionId);
        return Ok(transactions);
    }
}
