using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team.Core.Models;

namespace Team.Core.Interfaces
{
    public interface ITeamRepository
    {
        Task<List<TeamModel>> GetByCompanyIdAsync(string companyId);
        Task<TeamModel?> GetTeamByTeamLeadIdAsync(string teamLeadId);

        Task<TeamModel?> GetByIdAsync(string teamId);
        Task<TeamModel?> GetTeamByEmployeeIdAsync(string employeeId);
        Task CreateAsync(TeamModel team);
        Task UpdateAsync(TeamModel team);
        Task<bool> DeleteAsync(string teamId);

        Task<bool> ExistsByNameAsync(string companyId, string name);

        Task<bool> AddEmployeeToTeamAsync(string teamId, string employeeId);
        Task<bool> FindTeamEmployeeAsync(string teamId, string employeeId);
        Task<bool> RemoveEmployeeFromTeamAsync(string teamId, string employeeId);
    }
}

