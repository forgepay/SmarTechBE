namespace CryptoOnRamp.API.Controllers.PaymentLinks.Controllers;

internal sealed record PaymentLinksOptions
{
    public const string Position = "PaymentLinks";

    public decimal MinimumFiatAmount { get; init; } = 25;
    public string MinimumFiatAmountCurrency { get; init; } = "USD";
    public string DefaultFiatCurrency { get; init; } = "USD";
}