namespace MicPic.Infrastructure.Security.Internal;

internal sealed record SecurityOptions
{
    public const string Position = "Security";

    public string PasswordChars { get; init; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    public int PasswordLength { get; init; } = 32;
}