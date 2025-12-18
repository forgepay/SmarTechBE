using CryptoOnRamp.BLL.Models;
using CryptoOnRamp.DAL.Models;

namespace CryptoOnRamp.BLL.Interfaces;

public interface ITransactionService
{
    Task<IEnumerable<TransactionDto>> GetTransactionsAsync(int? userId, TransactionStatus? status, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize);
    Task<CheckoutSessionDTO?> GetSessionByIdAsync(string sessionId);
    Task<TransactionDto> GetTransactionAsync(int transactionId);
    Task<IEnumerable<PayoutDto>> GetPayoutsAsync(int? userId, PayoutStatusDb? status, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize);
    Task<PayoutDto> GetPayoutAsync(int payoutId);
}
