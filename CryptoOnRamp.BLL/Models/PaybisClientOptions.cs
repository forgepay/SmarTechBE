namespace CryptoOnRamp.BLL.Models;

public sealed class PaybisOptions : IHttpOptions
{
    public string PartnerId { get; set; } = "";
    public string HmacKey { get; set; } = ""; 
    public string PublicKey { get; set; } = ""; 
    public string Endpoint { get; set; } = "";
    public string SuccessReturnUrl { get; set; } = "";
    public string FailureReturnUrl { get; set; } = "";
    public string DefaultLocale { get; set; } = "en";
    public string ApiKey { get; set; } = string.Empty;

    public int RetryCount => throw new NotImplementedException();

    public TimeSpan RetryDelay => throw new NotImplementedException();

    public TimeSpan Timeout => throw new NotImplementedException();
}

public interface IHttpOptions
{
    string Endpoint { get; }
    int RetryCount { get; }
    TimeSpan RetryDelay { get; }
    TimeSpan Timeout { get; }
}
