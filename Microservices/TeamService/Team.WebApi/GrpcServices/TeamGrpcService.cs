using Grpc.Core;
using Team.Grpc;
using Team.Core.Interfaces;

namespace Team.WebApi.GrpcServices
{
    public class TeamGrpcServiceImpl : TeamGrpcService.TeamGrpcServiceBase
    {
        private readonly ITeamService _teamService;

        public TeamGrpcServiceImpl(ITeamService teamService)
        {
            _teamService = teamService;
        }

        public override async Task<TeamResponse> GetTeamById(GetTeamRequest request, ServerCallContext context)
        {
            var team = await _teamService.GetTeamAsync(request.TeamId, request.CompanyId);

            return new TeamResponse
            {
                Id = team.Id,
                Name = team.Name,
                CompanyId = team.CompanyId,
                TeamLeadId = team.TeamLeadId,
                Employees = { team.Employees }
            };
        }

        public override async Task<TeamBasicInfoResponse> GetTeamBasicInfo(GetTeamByIdRequest request, ServerCallContext context)
        {
            var team = await _teamService.InternalGetTeamAsync(request.TeamId);

            if (team == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Team not found"));

            return new TeamBasicInfoResponse
            {
                Name = team.Name,
                CompanyId = team.CompanyId
            };
        }


        public override async Task<TeamsListResponse> GetTeamsByCompanyId(GetCompanyTeamsRequest request, ServerCallContext context)
        {
            var teams = await _teamService.GetTeamsForCompanyAsync(request.CompanyId);

            var response = new TeamsListResponse();
            foreach (var t in teams)
            {
                response.Teams.Add(new TeamResponse
                {
                    Id = t.Id,
                    Name = t.Name,
                    CompanyId = t.CompanyId,
                    TeamLeadId = t.TeamLeadId,
                    Employees = { t.Employees }
                });
            }
            return response;
        }

        public override async Task<EmployeesResponse> GetTeamEmployees(GetTeamRequest request, ServerCallContext context)
        {
            var team = await _teamService.GetTeamAsync(request.TeamId, request.CompanyId);

            return new EmployeesResponse
            {
                EmployeeIds = { team.Employees }
            };
        }

        public override async Task<LeaderResponse> GetTeamLeader(GetTeamRequest request, ServerCallContext context)
        {
            var team = await _teamService.GetTeamAsync(request.TeamId, request.CompanyId);

            return new LeaderResponse
            {
                LeaderId = team.TeamLeadId
            };
        }
    }
}
