using AutoMapper;
using Team.Core.DTOs;
using Team.Core.Exceptions;
using Team.Core.Interfaces;
using Team.Core.Models;

namespace Team.Core.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _repo;
        private readonly IMapper _mapper;
        private readonly IGrpcAuthService _authService;
        public TeamService(ITeamRepository repo, IMapper mapper, IGrpcAuthService authService)
        {
            _repo = repo;
            _mapper = mapper;
            _authService = authService;
        }

        // Get Functions => get team team for Rest, get team for gRPC, get list, get list for AI

        public async Task<TeamResponseDto> GetTeamAsync(string teamId, string companyId)
        {
            var team = await _repo.GetByIdAsync(teamId);
            if (team == null || team.CompanyId != companyId)
                throw new NotFoundException("Team not found");
            var teamToReturn = _mapper.Map<TeamResponseDto>(team);
            var teamLeader = await _authService.GetUserByIdAsync(team.TeamLeadId);
            var employee = await _authService.GetUsersByIdsAsync(team.TeamEmployees.ToList());
            _mapper.Map(teamLeader, teamToReturn);
            _mapper.Map(employee, teamToReturn);
            return teamToReturn;
        }

        public async Task<TeamModel?> InternalGetTeamAsync(string teamId)
        {
            return await _repo.GetByIdAsync(teamId);
        }

        public async Task<List<TeamResponseDto>> GetTeamsForCompanyAsync(string companyId)
        {
            var teams = await _repo.GetByCompanyIdAsync(companyId);
            var teamToReturn = _mapper.Map<List<TeamResponseDto>>(teams);
            foreach (var team in teams)
            {
                var teamLeader = await _authService.GetUserByIdAsync(team.TeamLeadId);
                var employee = await _authService.GetUsersByIdsAsync(team.TeamEmployees.ToList());
                _mapper.Map(teamLeader, teamToReturn.First(t => t.Id == team.Id));
                _mapper.Map(employee, teamToReturn.First(t => t.Id == team.Id));
            }
            return teamToReturn;
        }

        public async Task<List<TeamResponseWithoutDetailsDto>> GetTeamsForAiAsync(string companyId)
        {
            var teams = await _repo.GetByCompanyIdAsync(companyId);

            return teams.Select(t => new TeamResponseWithoutDetailsDto
            {
                Id = t.Id,
                Name = t.Name
            }).ToList();
        }

        public async Task<TeamResponseDto> CreateTeamAsync(string companyId, CreateTeamDto dto)
        {
            if (await _repo.ExistsByNameAsync(companyId, dto.Name))
                throw new ConflictException("Team name already exists");

            if (dto.TeamLeadId == null)
                throw new ConflictException("Team leader required");

            var team = new TeamModel
            {
                Id = Guid.NewGuid().ToString(),
                CompanyId = companyId,
                Name = dto.Name,
                TeamLeadId = dto.TeamLeadId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.CreateAsync(team);

            return _mapper.Map<TeamResponseDto>(team);
        }

        public async Task<TeamResponseDto> UpdateTeamAsync(string teamId, string companyId, UpdateTeamDto dto)
        {
            var team = await _repo.GetByIdAsync(teamId);
            if (team == null || team.CompanyId != companyId)
                throw new NotFoundException("Team not found");

            if (dto.TeamLeadId == null)
                throw new ConflictException("Team leader required");

            if (await _repo.ExistsByNameAsync(companyId, dto.Name))
                throw new ConflictException("Duplicate team name");

            team.Name = dto.Name;
            team.TeamLeadId = dto.TeamLeadId;
            team.UpdatedAt = DateTime.UtcNow;


            await _repo.UpdateAsync(team);

            return _mapper.Map<TeamResponseDto>(team);
        }

        public async Task<bool> DeleteTeamAsync(string teamId, string companyId)
        {
            var team = await _repo.GetByIdAsync(teamId);

            if (team == null || team.CompanyId != companyId)
                throw new NotFoundException("Team not found");

            await _repo.DeleteAsync(teamId);

            return true;
        }


        // Employees

        public async Task<bool> AddEmployeeAsync(string teamId, string companyId, string employeeId)
        {
            var team = await _repo.GetByIdAsync(teamId);
            if (team == null || team.CompanyId != companyId)
                throw new NotFoundException("Team not found");

            return await _repo.AddEmployeeAsync(teamId, employeeId);
        }

        public async Task<bool> RemoveEmployeeAsync(string teamId, string companyId, string employeeId)
        {
            var team = await _repo.GetByIdAsync(teamId);
            if (team == null || team.CompanyId != companyId)
                throw new NotFoundException("Team not found");

            return await _repo.RemoveEmployeeAsync(teamId, employeeId);
        }

        public async Task<List<string>> GetTeamEmployeesAsync(string teamId)
        {
            return await _repo.GetTeamEmployeesAsync(teamId);
        }

        public async Task<string> GetTeamLeaderAsync(string teamId)
        {
            var team = await _repo.GetByIdAsync(teamId);
            if (team == null)
                throw new NotFoundException("Team not found");

            return team.TeamLeadId;
        }

        public async Task<TeamResponseDto?> GetTeamByLeaderIdAsync(string leaderId)
        {
            var team = await _repo.GetTeamByLeaderIdAsync(leaderId);
            return team == null ? null : _mapper.Map<TeamResponseDto>(team);
        }

    }
}
