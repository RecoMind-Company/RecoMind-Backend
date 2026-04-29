using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team.Core.DTOs;
using Team.Core.Interfaces;
using Team.Core.Models;
using Team.Infrastructure.Data;

namespace Team.Infrastructure.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly TeamDbContext _context;

        public TeamRepository(TeamDbContext context) => _context = context;

        private IQueryable<TeamModel> GetTeamsQuery(bool tracking = false)
            => tracking ? _context.Teams : _context.Teams.AsNoTracking();

        public async Task<TeamModel?> GetByIdAsync(string teamId)
            => await GetTeamsQuery(true).Include(t => t.TeamEmployees).FirstOrDefaultAsync(t => t.Id == teamId);

        public async Task<TeamModel?> GetTeamByEmployeeIdAsync(string employeeId)
            => await GetTeamsQuery().FirstOrDefaultAsync(t => t.TeamEmployees.Any(e => e.EmployeeId == employeeId));

        public async Task<TeamModel?> GetTeamByTeamLeadIdAsync(string teamLeadId)
            => await GetTeamsQuery().FirstOrDefaultAsync(t => t.TeamLeadId == teamLeadId);

        public async Task<List<TeamModel>> GetByCompanyIdAsync(string companyId)
            => await GetTeamsQuery().Where(t => t.CompanyId == companyId).Include(t => t.TeamEmployees).ToListAsync();

        public async Task<List<string>> GetTeamMemberIdsAsync(string teamId)
            => await _context.TeamEmployees.Where(te => te.TeamId == teamId)
                    .Select(te => te.EmployeeId)
                    .ToListAsync();

        public async Task CreateAsync(TeamModel team)
        {
            team.Id ??= Guid.NewGuid().ToString();
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TeamModel team)
        {
            _context.Teams.Update(team);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(string teamId)
                => await _context.Teams.Where(t => t.Id == teamId).ExecuteDeleteAsync() > 0;

        public async Task<bool> ExistsByNameAsync(string companyId, string name)
            => await _context.Teams.AnyAsync(t => t.CompanyId == companyId && t.Name == name);

        public async Task<bool> IsTeamBelongsToCompanyAsync(string teamId, string companyId)
            => await _context.Teams.AnyAsync(t => t.Id == teamId && t.CompanyId == companyId);

        // عمليات الموظفين (Simplified)
        public async Task<bool> AddEmployeeToTeamAsync(string teamId, string employeeId)
        {
            if (await IsEmployeeInTeam(teamId, employeeId)) return false;

            _context.TeamEmployees.Add(new TeamEmployee
            {
                Id = Guid.NewGuid().ToString(),
                TeamId = teamId,
                EmployeeId = employeeId
            });

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveEmployeeFromTeamAsync(string teamId, string employeeId)
            => await _context.TeamEmployees
                     .Where(x => x.TeamId == teamId && x.EmployeeId == employeeId)
                     .ExecuteDeleteAsync() > 0;

        public async Task<bool> IsEmployeeInTeam(string teamId, string employeeId)
            => await _context.TeamEmployees.AnyAsync(te => te.TeamId == teamId && te.EmployeeId == employeeId);
    }
}
