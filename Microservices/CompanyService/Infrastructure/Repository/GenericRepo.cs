using AutoMapper;
using Core.DTOs;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Data;
using Infrastructure.UnitOfWork;
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
        readonly CompanyDbContext _context;
        readonly DbSet<T> _dbSet;
        public GenericRepo(CompanyDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync() ?? new List<T>();
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            return await _dbSet.FindAsync(id); // tracked
        }

        public async Task<T?> GetByIdNoTrackingAsync(string id)
        {
            return await _dbSet.AsNoTracking()
                               .FirstOrDefaultAsync(e => EF.Property<string>(e, "Id") == id);
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public T Delete(T entity)
        {
            _dbSet.Remove(entity);
            return entity;
        }

        public async Task<T> Find(Expression<Func<T, bool>> predicate)
        {
           return await _dbSet.Where(predicate).FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<T>> FindAll(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
    }

}
