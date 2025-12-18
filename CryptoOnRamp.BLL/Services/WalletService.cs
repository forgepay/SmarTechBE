using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using CryptoOnRamp.DAL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using Nethereum.Web3;

namespace CryptoOnRamp.BLL.Services;

public class WalletService(
  IUserService currentUser,
  IUserRepository userRepo,
  ITransactionRepository txRepo,
  IOptions<BlockchainOptions> blockchainOptions) : IWalletService
{
    private readonly ITransactionRepository _transactionRepository = txRepo;
    private readonly IUserService _userService = currentUser;
    private readonly IUserRepository _userRepo = userRepo;
    private readonly BlockchainOptions _blockchainOptions = blockchainOptions.Value;
    private readonly string _usdcAbi = File.ReadAllText(blockchainOptions.Value.USDT.AbiPath);

    public (string Address, string PrivateKey) GenerateNewEthWallet()
    {
        var ecKey = Nethereum.Signer.EthECKey.GenerateKey();

        string privateKey = ecKey.GetPrivateKey();
        string address = ecKey.GetPublicAddress();

        return (address, privateKey);
    }

    public async Task<WalletBalanceDto> GetBalanceAsync(string address)
    {
        if (!IsEthAddress(address))
            throw new ArgumentException("Invalid address", nameof(address));

        var tx = await _transactionRepository.GetFirstOrDefaultAsync(x => x.UniqueWalletAddress == address);
        if (tx is null)
            throw new KeyNotFoundException("No transaction found for the given deposit address");


        var meId = _userService.GetCurrentUserId();
        var me = await _userRepo.GetFirstOrDefaultAsync(u => u.Id == meId)
                 ?? throw new UnauthorizedAccessException("User not found");

        switch (me.Role)
        {
            case UserRoleDb.Admin:
                break;

            case UserRoleDb.SuperAgent:
                var myAgents = await _userRepo.GetAgentsBySuperAgentIdAsync(me.Id);
                var allowed = myAgents.Any(a => a.Id == tx.UserId) || tx.UserId == me.Id;
                if (!allowed) throw new UnauthorizedAccessException("Forbidden: not your agent's wallet");
                break;

            case UserRoleDb.Agent:
                if (tx.UserId != me.Id) throw new UnauthorizedAccessException("Forbidden: not your wallet");
                break;

            default:
                throw new UnauthorizedAccessException("Forbidden");
        }

        var web3 = new Web3(_blockchainOptions.NetworkUrl);

        // USDC (ERC20 balanceOf)
        var contract = web3.Eth.GetContract(_usdcAbi, _blockchainOptions.USDT.ContractAddress);
        var balanceOf = contract.GetFunction("balanceOf");
        var rawUsdc = await balanceOf.CallAsync<System.Numerics.BigInteger>(address);

        int d = _blockchainOptions.USDT.Decimals; // 6
        var factor = System.Numerics.BigInteger.Pow(10, d);
        decimal usdc = (decimal)rawUsdc / (decimal)factor;
        usdc = Math.Round(usdc, d, MidpointRounding.AwayFromZero);

        // MATIC (native)
        var wei = await web3.Eth.GetBalance.SendRequestAsync(address); // корректно на Polygon
        var matic = Web3.Convert.FromWei(wei);

        return new WalletBalanceDto(address, usdc, matic);
    }

    public static bool IsEthAddress(string s) => !string.IsNullOrWhiteSpace(s) && s.StartsWith("0x") && s.Length == 42;
}
