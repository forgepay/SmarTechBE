using CryptoOnRamp.DAL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CryptoOnRamp.DAL.Repositories.Implementations;

public class TransactionRepository : EntityFrameworkRepository<TransactionDb, ApplicationContext>, ITransactionRepository
{
    public TransactionRepository(ApplicationContext context)
       : base(context)
    {
    }

    public async Task<TransactionDb?> GetByIdWithAllMetadataAsync(int transactionId)
    {
        return await DbSet
            .Include(x=> x.Payouts)
            .Include(x => x.CheckoutSessions)
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == transactionId);
    }

    public async Task<IEnumerable<TransactionDb>> GetTransactionsAsync(
        int? userId,
        TransactionStatusDb? status,
        DateTime? dateFrom,
        DateTime? dateTo,
        IEnumerable<int>? allowedUserIds = null,
        int? page = null,
        int? pageSize = null)
    {
        IQueryable<TransactionDb> query = DbSet.AsQueryable();

        if (userId.HasValue)
            query = query.Where(t => t.UserId == userId.Value);

        if (status != null)
            query = query.Where(t => t.Status == status);

        if (dateFrom.HasValue)
            query = query.Where(t => t.Timestamp >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(t => t.Timestamp <= dateTo.Value);

        if (allowedUserIds != null && allowedUserIds.Any())
            query = query.Where(t => allowedUserIds.Contains(t.UserId));

        if (page != null && pageSize != null)
        {
            query = query.OrderByDescending(p => p.Timestamp)
                .Skip((page.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
        }

        return await query
            .Include(t => t.Payouts)
            .Include(x=> x.CheckoutSessions)
            .Include(t => t.User)
            .AsNoTracking()
            .ToListAsync();
    }
}
