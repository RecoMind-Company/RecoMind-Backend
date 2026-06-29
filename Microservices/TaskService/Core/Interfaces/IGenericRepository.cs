using System.Linq.Expressions;

namespace Core.Interfaces;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> GetAllAsync();
    Task<IEnumerable<T>> FindAll(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
    Task<T?> Find(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Delete(T entity);
    Task BulkDeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}
