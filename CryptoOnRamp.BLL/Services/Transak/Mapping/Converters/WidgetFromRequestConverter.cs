using AutoMapper;
using CryptoOnRamp.BLL.Clients.Transak.Models;
using CryptoOnRamp.BLL.Services.LinkGenerator.Models;
using CryptoOnRamp.BLL.Services.Transak.Internal;
using MicPic.Infrastructure.Exceptions;
using Microsoft.Extensions.Options;

namespace CryptoOnRamp.BLL.Services.Transak.Mapping.Converters;

internal sealed class WidgetFromRequestConverter(IOptionsSnapshot<TransakServiceOptions> optionsSnapshot) : ITypeConverter<LinkRequest, WidgetRequest>
{
    private TransakServiceOptions _options { get; } = optionsSnapshot.Value;

    public WidgetRequest Convert(LinkRequest source, WidgetRequest destination, ResolutionContext context)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (!_options.Currencies.TryGetValue(source.CryptoCurrency, out var currency))
            throw new AppException("Unsupported currency.", BusinessErrorCodes.Unsupported);

        return new()
        {
            CryptoCurrency = currency.Symbol,
            FiatCurrency = source.FiatCurrency,
            FiatAmount = source.FiatAmount,
            Network = currency.Network,
            WalletAddress = source.WalletAddress,
            PartnerUserId = $"{source.UserId:D}",
            PartnerOrderId = $"{source.TransactionId:D}:transak",
        };
    }
}
