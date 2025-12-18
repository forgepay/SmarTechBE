using AutoMapper;
using CryptoOnRamp.API.Controllers.PaymentLinks.Dto;
using CryptoOnRamp.API.Filters;
using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using MicPic.Infrastructure.Mapping;
using MicPic.Infrastructure.RateLimit.Attributes;
using MicPic.Infrastructure.RateLimit.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CryptoOnRamp.API.Controllers.PaymentLinks.Controllers;

[Route("api/payment-links")]
[ApiController]
[ApiKeyAuthorization]
[AppRateLimitPolicy(AppRateLimitPolicyName.Default, Priority = int.MinValue)]
public class PaymentLinksController(
    IGenerateLinkService generateLinkService,
    IMapper mapper) : ControllerBase
{
    private readonly IGenerateLinkService _generateLinkService = generateLinkService;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// Generate payment links. Use API key to authorize.
    /// </summary>
    [HttpPost("generate")]
    [AppRateLimitPolicy(AppRateLimitPolicyName.PerIp)]
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
