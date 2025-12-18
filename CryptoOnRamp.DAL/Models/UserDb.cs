namespace CryptoOnRamp.DAL.Models;

public class UserDb
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; } = string.Empty;
    public string? Country { get; set; } = string.Empty;
    public UserRoleDb Role { get; set; }
    public string? PasswordHash { get; set; }
    public DateTime? CreatedAt { get; set; }
    public bool? EmailConfirmed { get; set; }
    public string? PasswordResetKey { get; set; }
    public int? CreatedById { get; set; }
    public string? Phone { get; set; } = string.Empty;
    public string? UsdcWallet { get; set; } = string.Empty;
    public bool? IsVerified { get; set; }
    public DateTime? DeletedAt { get; set; }
    public RegistrationStep? RegistrationStep { get; set; }
    public AppLanguage Language { get; set; } = AppLanguage.English;
    public string? WebhookUrl { get; set; }
    public string? WebhookAuthorizationToken { get; set; }
    public string? ApiKeyName { get; set; }
    public string? ApiKeyHash { get; set; }

    [Obsolete("Use Telegram Users instead")]
    public long? TelegramId { get; set; }
}

public enum RegistrationStep
{
    None,
    WaitingEmail,
    WaitingPhone,
    WaitingWallet,
    WaitingCurrency,
    WaitingAmount,
    Completed,
    WaitingLanguage,
    WaitingPassword
}

public enum AppLanguage
{
    English = 0,
    Russian= 1,
    Hebrew = 2,
    Portuguese = 3,
}
