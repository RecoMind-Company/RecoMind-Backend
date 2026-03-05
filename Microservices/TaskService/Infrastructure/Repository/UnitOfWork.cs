using Core.Interfaces;
using Infrastructure.Context;

namespace Infrastructure.Repository;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private Dictionary<string, object> _repositories = [];
    public IGenericRepository<T> GetRepository<T>() where T : class
    {
        var type = typeof(T).Name;
        if (!_repositories.ContainsKey(type))
        {
            var repositoryType = typeof(GenericRepository<>);
            var instance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), context);
            _repositories.Add(type, instance!);
        }
        return (IGenericRepository<T>)_repositories[type];
    }

    public async Task<int> SaveChangesAsync()
    {
        return await context.SaveChangesAsync();
    }
}
