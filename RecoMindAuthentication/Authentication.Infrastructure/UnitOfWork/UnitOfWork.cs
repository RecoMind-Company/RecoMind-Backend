using Authentication.Core.Interfaces;
using Authentication.Infrastructure.Context;
using Authentication.Infrastructure.Repository;

namespace Authentication.Infrastructure.UnitOfWork;

public class UnitOfWork<T>(AuthenticationDbContext context) : IUnitOfWork<T> where T : class
{
    private IGenericRepository<T> entity;
    public IGenericRepository<T> Entity => entity ?? (entity = new GenericRepository<T>(context));


    public async Task Save()
    {
        await context.SaveChangesAsync();
    }
}
