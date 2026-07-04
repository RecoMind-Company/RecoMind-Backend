using Core.Interfaces;
using Core.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class GenericRepo<T> : IGenericRepository<T> where T : class
    {
        private readonly SubscriptionDbContext _context;
        private readonly DbSet<T> Entity;
        public GenericRepo(SubscriptionDbContext context)
        {
            _context = context;
            Entity = _context.Set<T>();
        }
        public async Task<T> AddAsync(T entity)
        {
            await Entity.AddAsync(entity);
            return entity;
        }
        
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            if (typeof(T) == typeof(SubscriptionCompany))
            {
                var items = await _context.Set<SubscriptionCompany>()
                    .Include(s => s.subscriptionType)
                    .ToListAsync();
                return items.Cast<T>();
            }

            return await Entity.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            if (typeof(T) == typeof(SubscriptionCompany))
            {
                return await _context.Set<SubscriptionCompany>()
                    .Include(s => s.subscriptionType)
                    .FirstOrDefaultAsync(e => e.Id == id) as T;
            }

            return await Entity
                    .FirstOrDefaultAsync(e => EF.Property<string>(e, "Id") == id);
        }

        public async Task<T?> GetByIdNoTrackingAsync(string id)
        {
            return await Entity.AsNoTracking()
                    .FirstOrDefaultAsync(e => EF.Property<string>(e, "Id") == id);
        }        

        public T Update(T entity)
        {
            Entity.Update(entity);
            return entity;
        }

        public T Delete(T entity)
        {
            Entity.Remove(entity);
            return entity;
        }

        public async Task<T?> Find(Expression<Func<T, bool>> predicate)
        {
            return await Entity.FirstOrDefaultAsync(predicate);
        }
    }
}
