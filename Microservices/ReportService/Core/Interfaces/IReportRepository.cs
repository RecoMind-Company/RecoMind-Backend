using Core.Models;
using System.Linq.Expressions;

namespace Core.Interfaces;

public interface IReportRepository
{
    Task<IEnumerable<Report>> GetAllAsync();
    Task<Report?> GetByIdAsync(string id);
    Task AddAsync(Report report);
    void Update(Report report);
    void Delete(Report report);
    Task<Report?> Find(Expression<Func<Report, bool>> predicate);
    Task<IEnumerable<Report?>> FindAll(Expression<Func<Report, bool>> predicate);
}
