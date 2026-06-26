using System.Linq.Expressions;

namespace Core.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        public Task<T?> GetByIdNoTrackingAsync(string id);
        public Task<T> Find(Expression<Func<T, bool>> predicate);
        public Task<T?> Find(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
        public Task<T?> GetByIdAsync(string id);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        T Delete(T entity);
        Task<IEnumerable<T>> FindAll(Expression<Func<T, bool>> predicate);
    }
}
