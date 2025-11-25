using Core.Models;
using System.Linq.Expressions;

namespace Core.Interfaces;

public interface IInvitationRepository
{
    Task<IEnumerable<Invitation>> GetAllAsync();
    Task<Invitation?> GetByIdAsync(int id);
    Task CreateAsync(Invitation invitation);
    void Delete(Invitation invitation);
    Task<Invitation?> Find(Expression<Func<Invitation, bool>> predicate);
    Task<IEnumerable<Invitation?>> FindAll(Expression<Func<Invitation, bool>> predicate);
}
