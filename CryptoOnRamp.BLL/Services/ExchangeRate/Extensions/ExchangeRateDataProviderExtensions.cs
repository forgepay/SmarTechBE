using MicPic.Infrastructure.Exceptions;

namespace CryptoOnRamp.BLL.Services.ExchangeRate.Internal;

public static class ExchangeRateDataProviderExtensions
{
    public static async Task<decimal> GetExchangeRateAsync(this IExchangeRateDataProvider provider, string currency, string quote, CancellationToken cancellationToken)
    {
        var rate = await provider
            .GetExchangeRateOrDefaultAsync(currency, quote, cancellationToken);

        if (!rate.HasValue)
            throw new AppException($"Exchange rate {currency}/{quote} not found", BusinessErrorCodes.NotFound);

        return rate.Value;
    }

    public static async Task<decimal> ConvertAsync(this IExchangeRateDataProvider provider, string currency, string quote, decimal amount, CancellationToken cancellationToken)
    {
        if (amount == 0)
            return 0;

        if (string.Equals(currency, quote, StringComparison.OrdinalIgnoreCase))
            return amount;

        var rate = await provider
            .GetExchangeRateAsync(currency, quote, cancellationToken);

        return amount * rate;
    }
}
