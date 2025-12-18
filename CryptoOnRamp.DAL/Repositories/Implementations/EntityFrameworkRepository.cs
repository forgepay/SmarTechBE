using CryptoOnRamp.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CryptoOnRamp.DAL.Repositories.Implementations;

public class EntityFrameworkRepository<TEntity, TContext> : IRepository<TEntity>
 where TEntity : class
 where TContext : DbContext
{
    protected TContext Context;
    protected DbSet<TEntity> DbSet;

    public EntityFrameworkRepository(TContext contextWrapper)
    {
        Context = contextWrapper;
        DbSet = Context.Set<TEntity>();
    }

    public IQueryable<TEntity> Query() => DbSet.AsNoTracking();

    public async Task<IList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string includeProperties = "",
        bool asNoTracking = true,
        int? take = null)
    {
        IQueryable<TEntity> query = GetQuery(filter, orderBy, includeProperties);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        if (take != null)
        {
            query = query.Take(take.Value);
        }

        return await query.ToListAsync();
    }

    public IList<TEntity> Get(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string includeProperties = "",
        bool asNoTracking = true)
    {
        IQueryable<TEntity> query = GetQuery(filter, orderBy, includeProperties);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query.ToList();
    }

    public async Task<TEntity?> GetByIdAsync(object id)
    {
        return await DbSet.FindAsync(id);
    }

    public async Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>>? filter = null, string includeProperties = "", bool asNoTracking = true)
    {
        IQueryable<TEntity> query = GetQuery(filter, null, includeProperties);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<bool> IsAnyAsync(Expression<Func<TEntity, bool>>? filter = null)
    {
        IQueryable<TEntity> query = DbSet;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        return await query.AnyAsync();
    }

    public async Task InsertAsync(TEntity entity)
    {
        await DbSet.AddAsync(entity);
    }

    public void Insert(TEntity entity)
    {
        DbSet.Add(entity);
    }

    public async Task InsertRangeAsync(IEnumerable<TEntity> entities)
    {
        await DbSet.AddRangeAsync(entities);
    }

    public void InsertRange(IEnumerable<TEntity> entities)
    {
        DbSet.AddRange(entities);
    }

    public void Update(TEntity entityToUpdate)
    {
        DbSet.Attach(entityToUpdate);
        Context.Entry(entityToUpdate).State = EntityState.Modified;
    }

    public void UpdateRange(IEnumerable<TEntity> entitiesToUpdate)
    {
        DbSet.UpdateRange(entitiesToUpdate);
    }

    public void Delete(TEntity entityToDelete)
    {
        if (Context.Entry(entityToDelete).State == EntityState.Detached)
        {
            DbSet.Attach(entityToDelete);
        }

        DbSet.Remove(entityToDelete);
    }

    public void DeleteRange(IEnumerable<TEntity> entitiesToDelete)
    {
        DbSet.RemoveRange(entitiesToDelete);
    }

    public Task SaveAsync()
    {
        return Context.SaveChangesAsync();
    }

    public int Save()
    {
        return Context.SaveChanges();
    }

    private IQueryable<TEntity> GetQuery(
           Expression<Func<TEntity, bool>>? filter,
           Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy,
           string includeProperties)
    {
        IQueryable<TEntity> query = DbSet;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        query = includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Aggregate(query, (current, includeProperty) => current.Include(includeProperty));

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        return query;
    }
}