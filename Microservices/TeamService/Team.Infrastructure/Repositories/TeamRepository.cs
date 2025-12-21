using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team.Core.Interfaces;
using Team.Core.Models;
using Team.Infrastructure.Data;

namespace Team.Infrastructure.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly TeamDbContext _context;

        public TeamRepository(TeamDbContext context)
        {
            _context = context;
        }

        public async Task<TeamModel?> GetTeamByEmployeeIdAsync(string employeeId)
        {
            return await _context.Teams
                .Include(t => t.TeamEmployees)
                .FirstOrDefaultAsync(t =>
                    t.TeamEmployees.Any(e => e.EmployeeId == employeeId));
        }

        public async Task<TeamModel?> GetTeamByTeamLeadIdAsync(string teamLeadId)
        {
            return await _context.Teams
                .FirstOrDefaultAsync(t => t.TeamLeadId == teamLeadId);
        }

        public async Task<List<TeamModel>> GetByCompanyIdAsync(string companyId)
        {
            return await _context.Teams
                .AsNoTracking()
                .Where(t => t.CompanyId == companyId)
                .Include(t => t.TeamEmployees)
                .ToListAsync();
        }
        public async Task<TeamModel?> GetByIdAsync(string teamId)
        {
            return await _context.Teams
                .Include(t => t.TeamEmployees)
                .FirstOrDefaultAsync(t => t.Id == teamId);
        }

        public async Task CreateAsync(TeamModel team)
        {
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(TeamModel team) 
        {
            _context.Teams.Update(team);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> DeleteAsync(string teamId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team is null) return false;

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> ExistsByNameAsync(string companyId, string name)
        {
            return await _context.Teams
                .AnyAsync(t => t.CompanyId == companyId && t.Name == name);
        }


        public async Task<bool> AddEmployeeToTeamAsync(string teamId, string employeeId)
        {
            var valid = await FindTeamEmployeeAsync(teamId, employeeId);
            if (valid) return false;

            var relation = new TeamEmployee
            {
                Id = Guid.NewGuid().ToString(),
                TeamId = teamId,
                EmployeeId = employeeId
            };

            await _context.TeamEmployees.AddAsync(relation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> FindTeamEmployeeAsync(string teamId, string employeeId)
        {
            return await _context.TeamEmployees
                .AnyAsync(te => te.TeamId == teamId && te.EmployeeId == employeeId);
        }

        public async Task<bool> RemoveEmployeeFromTeamAsync(string teamId, string employeeId)
        {
            var relation = await _context.TeamEmployees
                .FirstOrDefaultAsync(x => x.TeamId == teamId && x.EmployeeId == employeeId);

            if (relation == null) return false;

            _context.TeamEmployees.Remove(relation);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
