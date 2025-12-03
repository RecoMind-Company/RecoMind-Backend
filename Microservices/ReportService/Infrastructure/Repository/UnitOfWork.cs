using Core.Interfaces;
using Infrastructure.Context;

namespace Infrastructure.Repository;

public class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
{
    public async Task Save()
    {
        await dbContext.SaveChangesAsync();
    }
}
