namespace CryptoOnRamp.BLL.Models;

public class OnramperOptions
{
    public static string Position { get; internal set; } = "Onramper";
    public string ApiKey { get; set; } = "";
    public string SignSecret { get; set; } = "";
    public string WebHookSecret { get; set; } = "";
    public string BaseUrl { get; set; } = "";
    public HashSet<string> DisabledProviders { get; set; } = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
}
