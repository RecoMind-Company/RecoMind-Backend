using Core.DTOs;
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
        Task<IEnumerable<T>> GetAllAsync();
        public Task<T?> GetByIdNoTrackingAsync(string id);
        public Task<T> Find(Expression<Func<T, bool>> predicate);
        public Task<IEnumerable<T>> FindAll(Expression<Func<T, bool>> predicate);
        public Task<T?> GetByIdAsync(string id);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        T Delete(T entity);
    }
}
