using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        public Task<T?> GetByIdAsync(string id);
        Task<T> AddAsync(T entity);
        T Delete(T entity);
        Task<List<T>> GetAllAsync();

        // to find by feature
        public Task<List<T>> FindAll(Expression<Func<T, bool>> predicate);

    }
}
