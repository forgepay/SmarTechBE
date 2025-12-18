using AutoMapper;
using CryptoOnRamp.API.Controllers.PaymentLinks.Controllers;
using CryptoOnRamp.API.Controllers.PaymentLinks.Dto;
using CryptoOnRamp.BLL.Models;
using CryptoOnRamp.BLL.Services.ExchangeRate.Internal;
using MicPic.Infrastructure.Exceptions;
using MicPic.Infrastructure.Extensions;
using MicPic.Infrastructure.Mapping;
using Microsoft.Extensions.Options;

namespace CryptoOnRamp.API.Controllers.PaymentLinks.Mapping.Converters;

internal sealed class GenerateLinkRequestFromDtoConverter(
    IExchangeRateDataProvider exchangeRateDataProvider,
    IOptionsSnapshot<PaymentLinksOptions> optionsSnapshot) : ITypeConverter<GenerateLinkRequestDto, Task<GenerateLinkRequest>>
{
    private readonly IExchangeRateDataProvider _exchangeRateDataProvider = exchangeRateDataProvider;
    private readonly PaymentLinksOptions _options = optionsSnapshot.Value;

    public async Task<GenerateLinkRequest> Convert(GenerateLinkRequestDto source, Task<GenerateLinkRequest> destination, ResolutionContext context)
    {
        var cancellationToken = context.GetCancellationToken();

        var fiatCurrency = source.FiatCurrency.NullIfWhiteSpace() ?? _options.DefaultFiatCurrency;
        
        var minimumAmount = await _exchangeRateDataProvider
            .ConvertAsync(_options.MinimumFiatAmountCurrency, fiatCurrency, _options.MinimumFiatAmount, cancellationToken);

        minimumAmount = Math.Round(minimumAmount, 2);

        if (source.Amount < minimumAmount)
        {
            throw new AppException($"Amount must be greater than or equal to {minimumAmount:0.##} {source.FiatCurrency}", BusinessErrorCodes.ValidationError);
        }

        return new()
        {
            UserId = source.UserId,
            Amount = source.Amount,
            FiatCurrency = fiatCurrency,
        };
    }
}