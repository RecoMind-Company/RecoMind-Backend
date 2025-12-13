using System.Linq.Expressions;

namespace Authentication.Core.Interfaces;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> Entities { get; }
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> GetAsync(int id);
    Task<T> AddAsync(T entity);
    T UpdateAsync(T entity);
    Task<bool> Any(Expression<Func<T, bool>> predicate);
    Task<List<T>> FindAll(Expression<Func<T, bool>> predicate);
    Task<T> Find(Expression<Func<T, bool>> predicate);
    void Delete(T entity);
}
