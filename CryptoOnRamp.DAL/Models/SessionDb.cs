namespace CryptoOnRamp.DAL.Models;

public class SessionDb
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public long Nonce { get; set; }

    public Dictionary<string, string> Claims { get; set; } = new();

    public DateTime ActivityAt { get; set; }

    public DateTime ExpiredAt { get; set; }
}
