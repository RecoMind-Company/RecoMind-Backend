using System.Linq.Expressions;

namespace Core.Interface;

public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> FindAll(Expression<Func<T, bool>> predicate,
                                              Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                                              params Expression<Func<T, object>>[] includes);
    Task<T?> Find(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}
