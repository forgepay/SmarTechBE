namespace CryptoOnRamp.BLL.Services.Transak.Internal;

internal sealed record TransakServiceOptionsCurrency
{
    public required string Network { get; init; }

    public required string Symbol { get; init; }
}