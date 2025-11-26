using Core.Interfaces;
using Infrastructure.Context;

namespace Infrastructure.Repository;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    public async Task Save()
    {
        await context.SaveChangesAsync();
    }
}
