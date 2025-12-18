namespace CryptoOnRamp.BLL.Models;

public class Session
{
    public string UserId { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public long Nonce { get; set; }

    public Dictionary<string, string> Claims { get; set; } = new();

    public DateTime ActivityAt { get; set; }

    public DateTime ExpiredAt { get; set; }

}
