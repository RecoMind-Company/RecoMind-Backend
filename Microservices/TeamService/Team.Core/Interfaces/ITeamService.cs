using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team.Core.DTOs;
using Team.Core.Models;
using Team.Core.Result;

namespace Team.Core.Interfaces
{
    public interface ITeamService
    {
        Task<List<TeamResponseForAiDto>> GetForAiAsync(string companyId);
        Task<List<TeamResponseDto>> GetByCompanyIdAsync(string companyId);
        Task<Result<TeamResponseDto>> GetByIdAsync(string teamId);
        Task<Result<UserTeamInfoDto>> GetTeamByEmployeeIdAsync(string employeeId);
        Task<Result<UserTeamInfoDto>> GetTeamByTeamLeadIdAsync(string teamLeadId);
        Task<List<UserJobTitleDto>> GetTeamMemberJobTitlesAsync(string teamId, string companyId);
        Task<List<string>> GetAllTeamEmployeesAsync(string teamId);
        Task<string> GetTeamLeaderAsync(string UserId);
        Task<List<string>> GetTeamEmployees(string teamLeadId);


        Task<Result<TeamResponseDto>> CreateTeamAsync(string companyId, CreateTeamDto dto);
        Task<Result<TeamResponseDto>> UpdateTeamAsync(string teamId, string companyId, UpdateTeamDto dto);
        Task<Result<bool>> DeleteTeamAsync(string teamId, string companyId);

        Task<Result<bool>> AddEmployeeAsync(string teamId, string companyId, string employeeId);
        Task<Result<bool>> RemoveEmployeeAsync(string teamId, string companyId, string employeeId);
        Task<bool> IsEmployeeInTeamAsync(string teamId, string employeeId);
    }
}
