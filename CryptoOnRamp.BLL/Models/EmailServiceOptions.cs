namespace CryptoOnRamp.BLL.Models;

public class EmailServiceOptions
{
    public const string Position = nameof(EmailServiceOptions);

    public string? SendGridApiKey { get; set; }
    public string SenderAddress { get; set; } = "";
    public string SenderName { get; set; } = "OnRamp";
}
