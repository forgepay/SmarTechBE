namespace CryptoOnRamp.BLL.Models;

public class BlockchainOptions
{
    public string NetworkUrl { get; set; } = string.Empty;
    public TokenOptions USDT { get; set; } = new();
    public string WebSocketUrl { get; internal set; } = string.Empty;
}

public class TokenOptions
{
    public string ContractAddress { get; set; } = string.Empty;
    public int Decimals { get; set; }
    public string AbiPath { get; set; } = string.Empty;

}
