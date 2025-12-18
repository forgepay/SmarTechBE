using AutoMapper;
using CryptoOnRamp.API.Controllers.PaymentLinks.Dto;
using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using MicPic.Infrastructure.Mapping;
using MicPic.Infrastructure.RateLimit.Attributes;
using MicPic.Infrastructure.RateLimit.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoOnRamp.API.Controllers.PaymentLinks.Controllers;

[Route("api/transactions")]
[ApiController]
[Authorize]
[AppRateLimitPolicy(AppRateLimitPolicyName.Default, Priority = int.MinValue)]
public class TransactionsController(
    IGenerateLinkService generateLinkService,
    IMapper mapper) : ControllerBase
{
    private readonly IGenerateLinkService _generateLinkService = generateLinkService;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// Generate payment links.
    /// </summary>
    [HttpPost("generate_link")]
    [AppRateLimitPolicy(AppRateLimitPolicyName.PerUser)]
    public async Task<ActionResult<GenerateLinkResponse>> GenerateAsync([FromBody] GenerateLinkRequestDto requestDto, CancellationToken cancellationToken)
    {
        var request = await _mapper
            .Map<Task<GenerateLinkRequest>>(
                source: requestDto,
                opts: opts => opts
                    .WithCancellationToken(cancellationToken));

        var response = await _generateLinkService
            .GenerateAsync(request, cancellationToken);

        return Ok(response);
    }    
}
