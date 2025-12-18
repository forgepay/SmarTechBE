namespace CryptoOnRamp.BLL.Services.ExchangeRate.Internal;

internal sealed record ExchangeRateServiceOptions
{
    public const string Position = "ExchangeRate";

    public TimeSpan CacheExpiry { get; init; } = TimeSpan.FromMinutes(30);
}
