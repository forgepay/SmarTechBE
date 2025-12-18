namespace CryptoOnRamp.BLL.Models;

public record WalletBalanceDto(string Address, decimal UsdcBalance, decimal MaticBalance);
