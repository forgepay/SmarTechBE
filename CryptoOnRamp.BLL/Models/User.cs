namespace CryptoOnRamp.BLL.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;
    public string UsdcWallet { get; set; } = string.Empty;
    public UserRole Role { get; internal set; }

    public int? CreatedById { get; set; }
    public string? WebhookUrl { get; set; }
}
