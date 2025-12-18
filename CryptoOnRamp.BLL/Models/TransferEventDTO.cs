using Nethereum.ABI.FunctionEncoding.Attributes;

namespace CryptoOnRamp.BLL.Models;

[Event("Transfer")]
public class TransferEventDTO : IEventDTO
{
    [Parameter("address", "from", 1, true)] public string From { get; set; } = "";
    [Parameter("address", "to", 2, true)] public string To { get; set; } = "";
    [Parameter("uint256", "value", 3, false)] public System.Numerics.BigInteger Value { get; set; }
}
