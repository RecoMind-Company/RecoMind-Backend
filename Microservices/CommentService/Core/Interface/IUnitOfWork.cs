namespace Core.Interface;

public interface IUnitOfWork
{
    IGenericRepository<T> GetRepository<T>() where T : class;
    Task<int> SaveChangesAsync();
}
