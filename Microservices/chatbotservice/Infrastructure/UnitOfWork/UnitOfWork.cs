using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.UnitOfWork
{
    public class UnitOfWork<T> : IUnitOfWork<T> where T : class
    {
        private readonly ChatbotDbContext _dbContext;
        private readonly IGenericRepository<T> _genericRepository;
        public UnitOfWork(ChatbotDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }
        public IGenericRepository<T> Entity
        {
            get
            {
                return _genericRepository ?? new GenericRepo<T>(_dbContext);
            }
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        public Task<int> Save()
        {
            return _dbContext.SaveChangesAsync();
        }
    }
}
