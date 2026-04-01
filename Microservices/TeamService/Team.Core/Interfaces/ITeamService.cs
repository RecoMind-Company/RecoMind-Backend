using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team.Core.DTOs;
using Team.Core.Models;

namespace Team.Core.Interfaces
{
    public interface ITeamService
    {
        Task<List<TeamResponseForAiDto>> GetForAiAsync(string companyId);
        Task<TeamResponseDto?> GetByIdAsync(string teamId);
        Task<List<TeamResponseDto>> GetByCompanyIdAsync(string companyId);
        Task<UserTeamInfoDto> GetUserTeamInfoAsync(string userId);
        Task<UserTeamInfoDto?> GetTeamByTeamLeadIdAsync(string teamLeadId);


        Task<TeamResponseDto> CreateTeamAsync(string companyId, CreateTeamDto dto);
        Task<TeamResponseDto> UpdateTeamAsync(string teamId, string companyId, UpdateTeamDto dto);
        Task<bool> DeleteTeamAsync(string teamId, string companyId);

        Task<bool> AddEmployeeAsync(string teamId, string companyId, string employeeId);
        Task<bool> RemoveEmployeeAsync(string teamId, string companyId, string employeeId);
        Task<bool> IsEmployeeInTeamAsync(string teamId, string employeeId);
    }
}
