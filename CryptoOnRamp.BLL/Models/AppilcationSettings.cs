namespace CryptoOnRamp.BLL.Models;

public class AppilcationSettings
{
    public static string Position { get; internal set; } = nameof(AppilcationSettings);
    public string UrlUi { get; set; } = string.Empty;
    public string UrlBackend { get; set; } = string.Empty;
}
