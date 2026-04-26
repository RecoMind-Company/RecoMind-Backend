using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.UnitOfWork
{
    public class UnitOfWork<T> : IUnitOfWork<T> where T : class
    {
        private readonly SubscriptionDbContext _context;
        private IGenericRepository<T>? _repository;
        public UnitOfWork(SubscriptionDbContext context )
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IGenericRepository<T> Entity
        {
            get
            {
                if (_repository == null)
                    _repository = new GenericRepo<T>(_context);

                return _repository;
            }
        }
        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }
    }
}
