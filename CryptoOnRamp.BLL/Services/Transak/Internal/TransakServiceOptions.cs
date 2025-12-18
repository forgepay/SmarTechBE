namespace CryptoOnRamp.BLL.Services.Transak.Internal;

internal sealed record TransakServiceOptions
{
    public const string Position = "Transak";

    public Dictionary<string, TransakServiceOptionsCurrency> Currencies { get; init; } = new(StringComparer.CurrentCultureIgnoreCase);
}
