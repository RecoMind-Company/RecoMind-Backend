using Core.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repository;

public class GenericRepository<T>(ApplicationDbContext context) : IGenericRepository<T> where T : class
{
    private readonly DbSet<T> _dbset = context.Set<T>();
    public IQueryable<T> GetAllAsync()
    {
        return _dbset.AsNoTracking();
    }
    public async Task<T?> Find(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbset;
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.FirstOrDefaultAsync(predicate);
    }
    public async Task<IEnumerable<T>> FindAll(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbset;
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.Where(predicate).ToListAsync();
    }
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbset.AnyAsync(predicate);
    }
    public async Task AddAsync(T entity)
    {
        await _dbset.AddAsync(entity);
    }
    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbset.AddRangeAsync(entities);
    }
    public void Update(T entity)
    {
        _dbset.Update(entity);
    }
    public void Delete(T entity)
    {
        _dbset.Remove(entity);
    }
    public async Task BulkDeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await _dbset.Where(predicate).ExecuteDeleteAsync(cancellationToken);
    }
}
