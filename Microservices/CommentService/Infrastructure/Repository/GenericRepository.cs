using Core.Interface;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repository;

public class GenericRepository<T>(ApplicationDbContext context) : IGenericRepository<T> where T : class
{
    private DbSet<T> _dbset = context.Set<T>();
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
