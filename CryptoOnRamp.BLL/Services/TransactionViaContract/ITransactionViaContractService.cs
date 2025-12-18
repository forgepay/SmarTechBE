using CryptoOnRamp.BLL.Models;
using CryptoOnRamp.DAL.Models;

namespace CryptoOnRamp.BLL.Services.TransactionViaContract;

public interface ITransactionViaContractService
{
    Task<TransactionDb> CreateTransactionAsync(int superAgentId, string superAgentWallet, string fiatCurrency, decimal amount, CancellationToken cancellationToken);
    Task<TransactionDb> CreateTransactionAsync(int superAgentId, string superAgentWallet, int agentId, string agentWallet, string fiatCurrency, decimal amount, CancellationToken cancellationToken);
    Task<PayoutResponse> PayoutAsync(int txId, CancellationToken cancellationToken);
}
