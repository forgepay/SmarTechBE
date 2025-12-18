namespace CryptoOnRamp.BLL.Services.LinkGenerator.Models;

public sealed record LinkResponse
{
    public required string OrderId { get; init; }
    public required string Link { get; init; }
    public required string Ramp { get; set; }
    public required string PaymentMethod { get; set; }
    public string? ExternalId { get; set; }
}