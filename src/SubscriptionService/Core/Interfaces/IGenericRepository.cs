using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        public Task<T?> GetByIdNoTrackingAsync(string id);
        public Task<T?> GetByIdAsync(string id);
        Task<T> AddAsync(T entity);
        T Update(T entity);
        T Delete(T entity);
    }
}
