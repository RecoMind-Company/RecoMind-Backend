using Core.Interfaces;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class UnitOfWork<T> : IUnitOfWork<T> where T : class
    {
        readonly PlanServiceDbContext _context;
        private IGenericRepository<T> _repository;
        public UnitOfWork(PlanServiceDbContext context)
        {
            _context = context;
        }
        public IGenericRepository<T> Entity
        {
            get
            {
                return _repository ?? new GenericRepo<T>(_context);
            }
        }
        public void Dispose()
        {
            _context.Dispose();
        }

        public Task<int> Save()
        {
            return _context.SaveChangesAsync();
        }       
    }
}
