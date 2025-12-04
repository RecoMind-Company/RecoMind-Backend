using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team.Core.Models;
using Team.Core.Interfaces;
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


        // READ: By Id
        public async Task<TeamModel?> GetByIdAsync(string teamId)
        {
            return await _context.Teams
                .AsNoTracking()
                .Include(t => t.TeamEmployees)
                .FirstOrDefaultAsync(t => t.Id == teamId && !t.IsDeleted);
        }

        // READ: By Company
        public async Task<List<TeamModel>> GetByCompanyIdAsync(string companyId)
        {
            return await _context.Teams
                .AsNoTracking()
                .Where(t => t.CompanyId == companyId && !t.IsDeleted)
                .Include(t => t.TeamEmployees)
                .ToListAsync();
        }

        // CREATE
        public async Task CreateAsync(TeamModel team)
        {
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();
        }

        // UPDATE
        public async Task UpdateAsync(TeamModel team)
        {
            var existing = await _context.Teams.FirstOrDefaultAsync(t => t.Id == team.Id);

            if (existing == null) return;

            existing.Name = team.Name;
            existing.TeamLeadId = team.TeamLeadId;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // SOFT DELETE
        public async Task DeleteAsync(string teamId)
        {
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null) return;

            team.IsDeleted = true;
            team.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // CHECK NAME EXISTS
        public async Task<bool> ExistsByNameAsync(string companyId, string name)
        {
            return await _context.Teams
                .AnyAsync(t => t.CompanyId == companyId &&
                    t.Name.ToLower() == name.ToLower() &&
                    !t.IsDeleted);
        }


        // EMPLOYEE: Add
        public async Task<bool> AddEmployeeAsync(string teamId, string employeeId)
        {
            bool exists = await _context.TeamEmployees
                .AnyAsync(te => te.TeamId == teamId && te.EmployeeId == employeeId);

            if (exists) return false;

            var entry = new TeamEmployee
            {
                Id = Guid.NewGuid().ToString(),
                TeamId = teamId,
                EmployeeId = employeeId,
                AddedAt = DateTime.UtcNow
            };

            await _context.TeamEmployees.AddAsync(entry);
            await _context.SaveChangesAsync();

            return true;
        }

        // EMPLOYEE: Remove
        public async Task<bool> RemoveEmployeeAsync(string teamId, string employeeId)
        {
            var te = await _context.TeamEmployees
                .FirstOrDefaultAsync(te => te.TeamId == teamId && te.EmployeeId == employeeId);

            if (te == null) return false;

            _context.TeamEmployees.Remove(te);
            await _context.SaveChangesAsync();

            return true;
        }


        // EMPLOYEE LIST
        public async Task<List<string>> GetTeamEmployeesAsync(string teamId)
        {
            return await _context.TeamEmployees
                .Where(e => e.TeamId == teamId)
                .Select(e => e.EmployeeId)
                .ToListAsync();
        }

        // GET BY TEAM LEADER
        public async Task<TeamModel?> GetTeamByLeaderIdAsync(string leaderId)
        {
            return await _context.Teams
                .AsNoTracking()
                .Where(t => t.TeamLeadId == leaderId && !t.IsDeleted)
                .Include(t => t.TeamEmployees)
                .FirstOrDefaultAsync();
        }

    }
}
