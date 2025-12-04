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
<<<<<<< HEAD
<<<<<<< Updated upstream
        public Task<T?> FindAsync(Expression<Func<T, bool>> predicate);
=======
        public Task<T> Find(Expression<Func<T, bool>> predicate);
>>>>>>> Stashed changes
=======

>>>>>>> b55309eff502b485ffbac0fff343644a670244ed
        public Task<T?> GetByIdAsync(string id);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        T Delete(T entity);
    }
}
