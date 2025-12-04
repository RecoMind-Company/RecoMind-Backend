using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repository;


namespace Infrastructure.UnitOfWork
{
    public sealed class unitOfWork<T> : IUnitOfWork<T>, IDisposable where T : class
    {
        private readonly CompanyDbContext _context;
        private IGenericRepository<T>? _repository;
        private bool _disposed;

        public unitOfWork(CompanyDbContext context)
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