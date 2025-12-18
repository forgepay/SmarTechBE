namespace CryptoOnRamp.BLL.Models;

public class PasswordResetInfo
{
    public string Email { get; set; } = string.Empty;
    public DateTime ExpiredAt { get; set; }
    public int UserId { get; internal set; }
}
