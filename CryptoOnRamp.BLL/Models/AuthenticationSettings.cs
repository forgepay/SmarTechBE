namespace CryptoOnRamp.BLL.Models;

public class AuthenticationSettings
{
    public static string Position { get; internal set; } = "AuthenticationSettings";
    public string PrivateKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "Issuer";
    public string Audience { get; set; } = "Audience";
    public TimeSpan EmailAuthenticationCodeLifeTime { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan PhoneNumberAuthenticationCodeLifeTime { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan EmailRecoveryCodeLifeTime { get; set; } = TimeSpan.FromMinutes(5);
    public int MaxSessions { get; set; } = 1000;
    public TimeSpan LockLifeTime { get; set; } = TimeSpan.FromSeconds(5);
}
