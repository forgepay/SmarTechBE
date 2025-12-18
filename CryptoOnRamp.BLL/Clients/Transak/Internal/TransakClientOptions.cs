namespace CryptoOnRamp.BLL.Clients.Transak.Internal;

internal sealed record TransakClientOptions
{
    public const string Position = "Transak";

    public string Endpoint { get; init; } = string.Empty;

    public string GatewayEndpoint { get; init; } = string.Empty;

    public string ApiEnvironment { get; set; } = string.Empty;

    public string ApiKey { get; init; } = string.Empty;

    public string ApiSecret{ get; init; } = string.Empty;

    public string ReferrerDomain { get; set; } = string.Empty;

    public string RedirectUrl { get; set; } = string.Empty;

    public string LandingPage { get; set; } = "Home";
}
