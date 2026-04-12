using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.UnitOfWork
{
    public sealed class UOW<T> : IUnitOfWork<T>, IDisposable where T : class
    {
        private readonly DbContext _context;
        private IGenericRepository<T>? _repository;
        private bool _disposed;

        public UOW(PlanDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IGenericRepository<T> Entity => _repository ??= new GenericRepo<T>(_context);

        public void Save()
        {
            _context.SaveChanges();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }
}
