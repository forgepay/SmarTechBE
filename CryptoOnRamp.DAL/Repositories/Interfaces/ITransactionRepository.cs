using CryptoOnRamp.DAL.Models;

namespace CryptoOnRamp.DAL.Repositories.Interfaces;

public interface ITransactionRepository : IRepository<TransactionDb>
{
    Task<TransactionDb?> GetByIdWithAllMetadataAsync(int transactionId);

    Task<IEnumerable<TransactionDb>> GetTransactionsAsync(
        int? userId,
        TransactionStatusDb? status,
        DateTime? dateFrom,
        DateTime? dateTo,
        IEnumerable<int>? allowedUserIds = null,
        int? page = null,
        int? pageSize = null);
}
