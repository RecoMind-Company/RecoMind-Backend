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
        // Create, Read, Update, Delete methods for Team entities can be defined here

        Task<TeamModel?> GetByIdAsync(string teamId);
        Task<List<TeamModel>> GetByCompanyIdAsync(string companyId);
        Task CreateAsync(TeamModel team);
        Task UpdateAsync(TeamModel team);
        Task DeleteAsync(string teamId);
        Task<bool> ExistsByNameAsync(string companyId, string name);

        // Employees
        Task<bool> AddEmployeeAsync(string teamId, string employeeId);
        Task<bool> RemoveEmployeeAsync(string teamId, string employeeId);
        Task<List<string>>GetTeamEmployeesAsync(string teamId);
        Task<TeamModel?> GetTeamByLeaderIdAsync(string teamLeadId);

    }
}

