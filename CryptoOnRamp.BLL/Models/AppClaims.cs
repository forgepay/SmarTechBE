namespace CryptoOnRamp.BLL.Models;

public static class AppClaims
{
    public const string ClaimPrefix = "X-SC-";

    public const string Otc = ClaimPrefix + "otc";
    public const string TokenType = ClaimPrefix + "TokenType";
    public const string Id = ClaimPrefix + "ID";
    public const string Session = ClaimPrefix + "Session-ID";
    public const string Nonce = ClaimPrefix + "Nonce";
    public const string Role = ClaimPrefix + "Role";
}
