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
        Task<TeamResponseDto> GetTeamAsync(string teamId, string companyId);
        Task<List<TeamResponseDto>> GetTeamsForCompanyAsync(string companyId);
        Task<List<TeamResponseWithoutDetailsDto>> GetTeamsForAiAsync(string companyId);
        Task<TeamModel?> InternalGetTeamAsync(string teamId);

        Task<TeamResponseDto> CreateTeamAsync(string companyId, CreateTeamDto dto);
        Task<TeamResponseDto> UpdateTeamAsync(string teamId, string companyId, UpdateTeamDto dto);
        Task<bool> DeleteTeamAsync(string teamId, string companyId);


        Task<bool> AddEmployeeAsync(string teamId, string companyId, string employeeId);
        Task<bool> RemoveEmployeeAsync(string teamId, string companyId, string employeeId);
        Task<List<string>> GetTeamEmployeesAsync(string teamId);
        Task<string> GetTeamLeaderAsync(string teamId);
        Task<TeamResponseDto?> GetTeamByLeaderIdAsync(string leaderId);

    }
}
