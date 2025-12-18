namespace CryptoOnRamp.BLL.Models;

public class IdentityServiceOptions
{
    public const string Position = "IdentityService";

    public string PhoneNumberAuthenticationCodePattern { get; set; } = "000000";
    public string EmailRecoveryCodePattern { get; set; } = "AAA-000";
    public TimeSpan AccessTokenLifeTime { get; set; } = TimeSpan.FromMinutes(80);
    public TimeSpan RefreshTokenLifeTime { get; set; } = TimeSpan.FromDays(14);
}
