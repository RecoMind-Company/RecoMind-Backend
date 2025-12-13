using AutoMapper;
using Team.Core.DTOs;
using Team.Core.Interfaces;
using Team.Core.Models;

namespace Team.Core.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _repo;
        private readonly IMapper _mapper;

        public TeamService(ITeamRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<TeamResponseForAiDto>> GetForAiAsync(string companyId)
        {
            var teams = await _repo.GetByCompanyIdAsync(companyId);
            return _mapper.Map<List<TeamResponseForAiDto>>(teams);
        }

        public async Task<List<TeamResponseDto>> GetByCompanyIdAsync(string companyId)
        {
            var teams = await _repo.GetByCompanyIdAsync(companyId);
            return _mapper.Map<List<TeamResponseDto>>(teams);
        }

        public async Task<TeamResponseDto?> GetByIdAsync(string teamId)
        {
            var team = await _repo.GetByIdAsync(teamId);
            if (team == null) return null;

            return _mapper.Map<TeamResponseDto>(team);
        }

        public async Task<TeamResponseDto> CreateTeamAsync(string companyId, CreateTeamDto dto)
        {
            if (await _repo.ExistsByNameAsync(companyId, dto.Name))
                throw new Exception("Team with the same name already exists.");

            var team = _mapper.Map<TeamModel>(dto);
            team.Id = Guid.NewGuid().ToString();
            team.CompanyId = companyId;
            team.CreatedAt = DateTime.UtcNow;
            team.UpdatedAt = DateTime.UtcNow;

            await _repo.CreateAsync(team);
            return _mapper.Map<TeamResponseDto>(team);
        }

        public async Task<TeamResponseDto> UpdateTeamAsync(string teamId, string companyId, UpdateTeamDto dto)
        {
            var team = await _repo.GetByIdAsync(teamId);
            if (team == null || team.CompanyId != companyId)
                throw new Exception("Team not found.");

            if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != team.Name)
            {
                if (await _repo.ExistsByNameAsync(companyId, dto.Name))
                    throw new Exception("Another team with the same name exists.");
            }

            _mapper.Map(dto, team);
            team.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(team);
            return _mapper.Map<TeamResponseDto>(team);
        }

        public async Task<bool> DeleteTeamAsync(string teamId, string companyId)
        {
            var team = await _repo.GetByIdAsync(teamId);
            if (team == null || team.CompanyId != companyId)
                return false;

            return await _repo.DeleteAsync(teamId);
        }

        public async Task<bool> AddEmployeeAsync(string teamId, string companyId, string employeeId)
        {
            var team = await _repo.GetByIdAsync(teamId);
            if (team == null || team.CompanyId != companyId)
                return false;

            if (team.TeamEmployees.Any(e => e.EmployeeId == employeeId))
                return false;

            return await _repo.AddEmployeeToTeamAsync(teamId, employeeId);
        }

        public async Task<bool> RemoveEmployeeAsync(string teamId, string companyId, string employeeId)
        {
            var team = await _repo.GetByIdAsync(teamId);
            if (team == null || team.CompanyId != companyId)
                return false;

            return await _repo.RemoveEmployeeFromTeamAsync(teamId, employeeId);
        }
    }

}
