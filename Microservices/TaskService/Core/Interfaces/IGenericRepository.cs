using System.Linq.Expressions;

namespace Core.Interfaces;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> GetAllAsync(params Expression<Func<T, object>>[] includes);
    Task<IEnumerable<T>> FindAll(Expression<Func<T, bool>> predicate);
    Task<T?> Find(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}
