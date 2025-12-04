namespace Authentication.Core.Interfaces;

public interface IUnitOfWork<T> where T : class
{
    public IGenericRepository<T> Entity { get; }
    Task Save();
}
