using CryptoOnRamp.DAL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CryptoOnRamp.DAL.Repositories.Implementations;

public class PayoutRepository : EntityFrameworkRepository<PayoutDb, ApplicationContext>, IPayoutRepository
{
    public PayoutRepository(ApplicationContext context)
       : base(context)
    {
    }

    public async Task<PayoutDb?> GetByIdWithAllMetadataAsync(int payoutId)
    {
        return await DbSet
            .Include(x => x.Transaction)
            .FirstOrDefaultAsync(x => x.Id == payoutId);
    }

    public async Task<IEnumerable<PayoutDb>> GetPayoutsAsync(
        int? userId,
        PayoutStatusDb? status,
        DateTime? dateFrom,
        DateTime? dateTo,
        IEnumerable<int>? allowedUserIds = null,
        int? page = null,
        int? pageSize = null)
    {
        var q = DbSet.AsQueryable();

        q = q.Include(p => p.Transaction);

        if (userId.HasValue)
            q = q.Where(p => p.Transaction.UserId == userId.Value);

        if (status.HasValue)
            q = q.Where(p => p.Status == status.Value);

        if (dateFrom.HasValue)
            q = q.Where(p => p.CreatetAt >= dateFrom.Value);

        if (dateTo.HasValue)
            q = q.Where(p => p.CreatetAt < dateTo.Value);

        if (allowedUserIds != null)
            q = q.Where(p => allowedUserIds.Contains(p.Transaction.UserId));

        if (page != null && pageSize != null)
        {
            q = q.OrderBy(p => p.CreatetAt)
                .Skip((page.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
        }


        return await q.ToListAsync();
    }
}
