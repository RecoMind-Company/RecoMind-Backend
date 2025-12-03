using Core.Interfaces;
using Core.Models;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repository;

public class ReportRepository(ApplicationDbContext dbContext) : IReportRepository
{
    private readonly DbSet<Report> _reports = dbContext.Reports;
    public async Task<IEnumerable<Report>> GetAllAsync()
    {
        return await _reports.AsNoTracking().ToListAsync();
    }
    public async Task<Report?> GetByIdAsync(string id)
    {
        return await _reports.FindAsync(id);
    }
    public async Task AddAsync(Report report)
    {
        await _reports.AddAsync(report);
    }
    public void Update(Report report)
    {
        _reports.Update(report);
    }
    public void Delete(Report report)
    {
        _reports.Remove(report);
    }
    public async Task<Report?> Find(Expression<Func<Report, bool>> predicate)
    {
        return await _reports.FirstOrDefaultAsync(predicate);
    }
    public async Task<IEnumerable<Report?>> FindAll(Expression<Func<Report, bool>> predicate)
    {
        return await _reports.Where(predicate).ToListAsync();
    }
}
