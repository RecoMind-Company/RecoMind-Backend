using AutoMapper;
using Team.Core.DTOs;
using Team.Core.Interfaces;
using Team.Core.Models;
using Team.Core.Result;

namespace Team.Core.Services
{
    public class TeamService : ITeamService
    {
        private readonly IAuthGrpcService _authGrpcService;
        private readonly ITeamRepository _repo;
        private readonly IMapper _mapper;

        public TeamService(IAuthGrpcService authGrpcService, ITeamRepository repo, IMapper mapper)
        {
            _authGrpcService = authGrpcService;
            _mapper = mapper;
            _repo = repo;
        }

        #region Helper Methos
        private Result<UserTeamInfoDto> MapToUserTeamInfo(TeamModel? team)
        {
            if (team == null) return TeamErrors.NotFound;

            return Result<UserTeamInfoDto>.Success(new UserTeamInfoDto
            {
                CompanyId = team.CompanyId,
                TeamId = team.Id,
                TeamName = team.Name
            });
        }
        #endregion

        #region Read Operations
        public async Task<Result<UserTeamInfoDto>> GetTeamByTeamLeadIdAsync(string teamLeadId)
        => MapToUserTeamInfo(await _repo.GetTeamByTeamLeadIdAsync(teamLeadId));

        public async Task<Result<UserTeamInfoDto>> GetTeamByEmployeeIdAsync(string employeeId)
            => MapToUserTeamInfo(await _repo.GetTeamByEmployeeIdAsync(employeeId));

        public async Task<List<TeamResponseForAiDto>> GetForAiAsync(string companyId)
        {
            var teams = await _repo.GetByCompanyIdAsync(companyId);
            return teams == null ? new List<TeamResponseForAiDto>()
                : _mapper.Map<List<TeamResponseForAiDto>>(teams);
        }

        public async Task<List<TeamResponseDto>> GetByCompanyIdAsync(string companyId)
        {
            var teams = await _repo.GetByCompanyIdAsync(companyId);
            return teams == null ? new List<TeamResponseDto>()
                : _mapper.Map<List<TeamResponseDto>>(teams);
        }

        public async Task<Result<TeamResponseDto>> GetByIdAsync(string teamId)
        {
            var team = await _repo.GetByIdAsync(teamId);
            if (team == null) return TeamErrors.NotFound;

            return _mapper.Map<TeamResponseDto>(team);
        }

        public async Task<List<string>> GetTeamMemberJobTitlesAsync(string teamId, string companyId)
        {
            var teamExist = await _repo.IsTeamBelongsToCompanyAsync(teamId, companyId);
            var employeeIds = await _repo.GetTeamMemberIdsAsync(teamId);

            if (!teamExist || employeeIds == null || !employeeIds.Any())
                return new List<string>();

            var grpcResult = await _authGrpcService.GetTeamEmployeesJobTitlesAsync(employeeIds);

            if (!grpcResult.IsSuccess || grpcResult.Value == null)
                return new List<string>();

            return grpcResult.Value
                .Select(x => x.JobTitle)
                .Where(title => !string.IsNullOrWhiteSpace(title))
                .Distinct()
                .ToList();
        }
        #endregion

        public async Task<List<string>> GetAllTeamEmployeesAsync(string teamId)
        {
            var employeeIds = await _repo.GetTeamMemberIdsAsync(teamId);

            return employeeIds ?? new List<string>();
        }


        #region Write Operations
        public async Task<Result<TeamResponseDto>> CreateTeamAsync(string companyId, CreateTeamDto dto)
        {
            if (await _repo.ExistsByNameAsync(companyId, dto.Name))
                return TeamErrors.NameAlreadyExists;

            var team = _mapper.Map<TeamModel>(dto);
            team.Id = Guid.NewGuid().ToString();
            team.CompanyId = companyId;
            team.CreatedAt = DateTime.UtcNow;

            await _repo.CreateAsync(team);
            return _mapper.Map<TeamResponseDto>(team);
        }

        public async Task<Result<TeamResponseDto>> UpdateTeamAsync(string teamId, string companyId, UpdateTeamDto dto)
        {
            var team = await _repo.GetByIdAsync(teamId);
            if (team == null || team.CompanyId != companyId)
                return TeamErrors.NotFound;

            if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != team.Name)
            {
                if (await _repo.ExistsByNameAsync(companyId, dto.Name))
                    return TeamErrors.NameAlreadyExists;
            }

            _mapper.Map(dto, team);
            team.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(team);
            return _mapper.Map<TeamResponseDto>(team);
        }

        public async Task<Result<bool>> DeleteTeamAsync(string teamId, string companyId)
        {
            var team = await _repo.GetByIdAsync(teamId);
            if (team == null || team.CompanyId != companyId)
                return TeamErrors.NotFound;

            return await _repo.DeleteAsync(teamId);
        }
        #endregion

        #region Employee Management

        public async Task<Result<bool>> AddEmployeeAsync(string teamId, string companyId, string employeeId)
        {
            var team = await _repo.GetByIdAsync(teamId);
            if (team == null || team.CompanyId != companyId)
                return TeamErrors.NotFound;

            if (await _repo.IsEmployeeInTeam(teamId, employeeId))
                return TeamErrors.EmployeeAlreadyInTeam;

            return await _repo.AddEmployeeToTeamAsync(teamId, employeeId);
        }

        public async Task<Result<bool>> RemoveEmployeeAsync(string teamId, string companyId, string employeeId)
        {
            var team = await _repo.GetByIdAsync(teamId);
            if (team == null || team.CompanyId != companyId)
                return TeamErrors.NotFound;

            var success = await _repo.RemoveEmployeeFromTeamAsync(teamId, employeeId);
            return success;
        }

        public async Task<bool> IsEmployeeInTeamAsync(string teamId, string employeeId)
            => await _repo.IsEmployeeInTeam(teamId, employeeId);
        #endregion
    }
}
