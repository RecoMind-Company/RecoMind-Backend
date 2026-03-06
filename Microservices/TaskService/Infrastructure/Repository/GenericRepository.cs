using Core.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repository;

public class GenericRepository<T>(ApplicationDbContext context) : IGenericRepository<T> where T : class
{
    private readonly DbSet<T> _dbset = context.Set<T>();
    public IQueryable<T> GetAllAsync(params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbset.AsNoTracking();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return query;
    }
    public async Task<T?> Find(Expression<Func<T, bool>> predicate)
    {
        return await _dbset.FirstOrDefaultAsync(predicate);
    }
    public async Task<IEnumerable<T>> FindAll(Expression<Func<T, bool>> predicate)
    {
        return await _dbset.Where(predicate).ToListAsync();
    }
    public async Task AddAsync(T entity)
    {
        await _dbset.AddAsync(entity);
    }
    public void Update(T entity)
    {
        _dbset.Update(entity);
    }
    public void Delete(T entity)
    {
        _dbset.Remove(entity);
    }
}
