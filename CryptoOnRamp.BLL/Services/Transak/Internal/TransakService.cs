using System.Runtime.CompilerServices;
using AutoMapper;
using CryptoOnRamp.BLL.Clients.Transak;
using CryptoOnRamp.BLL.Clients.Transak.Models;
using CryptoOnRamp.BLL.Services.LinkGenerator.Models;
using Microsoft.Extensions.Logging;

namespace CryptoOnRamp.BLL.Services.Transak.Internal;

internal sealed partial class TransakService(ITransakGatewayClient transakGatewayClient, IMapper mapper, ILogger<TransakService> logger) : ITransakService
{
    private readonly ITransakGatewayClient _transakGatewayClient = transakGatewayClient;
    private readonly IMapper _mapper = mapper;


    #region ITransakService Implementation

    public async Task<LinkResponse> GenerateLinkAsync(LinkRequest request, CancellationToken cancellationToken)
    {
        var widgetRequest = _mapper
            .Map<WidgetRequest>(request);

        var widgetResponse = await _transakGatewayClient
            .BuildWidgetAsync(widgetRequest, cancellationToken);

        var linkResponse = _mapper
            .Map<LinkResponse>(widgetResponse);

        return linkResponse;
    }

    #endregion


    #region ILinkSource Implementation

    public async IAsyncEnumerable<LinkResponse> GenerateLinksAsync(LinkRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var linkResponse = await GenerateLinkOrDefaultAsync(request, cancellationToken);

        if (linkResponse is null)
            yield break;

        yield return linkResponse;
    }
    
    #endregion


    #region Private Methods

    private async Task<LinkResponse?> GenerateLinkOrDefaultAsync(LinkRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var linkResponse = await GenerateLinkAsync(request, cancellationToken);

            LogLinkSuccessfullyGenerated(linkResponse.Link);

            return linkResponse;
        }
        catch (Exception ex)
        {
            LogLinkGenerationFailed(ex.Message);

            return null;
        }
    }

    #endregion


    #region Logging

    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Information,
        Message = "Transak link successfully generated: {link}")]
    private partial void LogLinkSuccessfullyGenerated(string link);

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Failed to generate Transak link: {errorMessage}")]
    private partial void LogLinkGenerationFailed(string errorMessage);

    #endregion
}