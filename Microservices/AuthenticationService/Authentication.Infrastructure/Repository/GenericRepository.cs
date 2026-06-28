using Authentication.Core.Interfaces;
using Authentication.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Authentication.Infrastructure.Repository;

public class GenericRepository<T>(AuthenticationDbContext context) : IGenericRepository<T> where T : class
{
    private DbSet<T> _dbSet = context.Set<T>();
    public IQueryable<T> Entities => _dbSet;
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }
    public async Task<T> GetAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }
    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public async Task<bool> Any(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }


    public async Task<T> Find(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public Task<List<T>> FindAll(Expression<Func<T, bool>> predicate)
    {
        return _dbSet.Where(predicate).ToListAsync();
    }

    public async Task<T> Find(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
    {
        var query = _dbSet.AsQueryable();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.FirstOrDefaultAsync(predicate);
    }
    public T UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return entity;
    }
    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

}
