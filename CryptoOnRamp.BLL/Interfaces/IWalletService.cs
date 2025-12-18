using CryptoOnRamp.BLL.Models;

namespace CryptoOnRamp.BLL.Interfaces;

public interface IWalletService
{
    (string Address, string PrivateKey) GenerateNewEthWallet();
    Task<WalletBalanceDto> GetBalanceAsync(string address);
}
