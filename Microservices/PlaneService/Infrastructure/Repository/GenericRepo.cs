using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Grpc.Core.Metadata;

namespace Infrastructure.Repository
{
    public class GenericRepo<T> : IGenericRepository<T> where T : class
    {
        private readonly PlanServiceDbContext _dbContext;
        private readonly DbSet<T> _dbSet;
        public GenericRepo(PlanServiceDbContext planServiceDbContext)
        {
            _dbContext = planServiceDbContext;
            _dbSet = _dbContext.Set<T>();
        }
        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public T Delete(T entity)
        {
            _dbSet.Remove(entity);
            return entity;
        }

        public async Task<List<T>> FindAll(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public Task<List<T>> GetAllAsync()
        {
            return _dbSet.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            return await _dbSet.AsNoTracking()
                    .FirstOrDefaultAsync(e => EF.Property<string>(e, "Id") == id);
        }

        public T Update(T entity)
        {
            _dbSet.Update(entity);
            return entity;
        }
    }
}
