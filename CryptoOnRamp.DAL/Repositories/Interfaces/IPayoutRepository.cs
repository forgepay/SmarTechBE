using CryptoOnRamp.DAL.Models;

namespace CryptoOnRamp.DAL.Repositories.Interfaces;

public interface IPayoutRepository : IRepository<PayoutDb>
{
    Task<IEnumerable<PayoutDb>> GetPayoutsAsync(
        int? userId,
        PayoutStatusDb? status,
        DateTime? dateFrom,
        DateTime? dateTo,
        IEnumerable<int>? allowedUserIds = null,
        int? page = null,
        int? pageSize = null);

    Task<PayoutDb?> GetByIdWithAllMetadataAsync(int payoutId);
}
