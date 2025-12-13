using Grpc.Core;
using Team.Core.Interfaces;
using Team.WebApi.Protos;

namespace Team.WebApi.GrpcServices
{
    public class TeamGrpcServiceImpl : TeamGrpcService.TeamGrpcServiceBase
    {
        private readonly ITeamService _teamService;

        public TeamGrpcServiceImpl(ITeamService teamService)
        {
            _teamService = teamService;
        }

        public override async Task<TeamResponse> GetTeamById(
            GetTeamByIdRequest request,
            ServerCallContext context)
        {
            var team = await _teamService.GetByIdAsync(request.TeamId);

            if (team == null)
                throw new RpcException(
                    new Status(StatusCode.NotFound, "Team not found"));

            return new TeamResponse
            {
                Id = team.Id,
                Name = team.Name,
                CompanyId = team.CompanyId,
                TeamLeadId = team.TeamLeadId,
                Employees = { team.Employees }
            };
        }

        public override async Task<TeamsListResponse> GetTeamsByCompanyId(
            GetCompanyTeamsRequest request,
            ServerCallContext context)
        {
            var teams = await _teamService.GetByCompanyIdAsync(request.CompanyId);

            var response = new TeamsListResponse();
            foreach (var team in teams)
            {
                response.Teams.Add(new TeamResponse
                {
                    Id = team.Id,
                    Name = team.Name,
                    CompanyId = team.CompanyId,
                    TeamLeadId = team.TeamLeadId,
                    Employees = { team.Employees }
                });
            }

            return response;
        }

        public override async Task<UserTeamInfoResponse> GetUserTeamInfo(
            GetUserTeamInfoRequest request,
            ServerCallContext context)
        {
            var info = await _teamService.GetUserTeamInfoAsync(request.UserId);

            if (info == null)
                throw new RpcException(
                    new Status(StatusCode.NotFound, "User team info not found"));

            return new UserTeamInfoResponse
            {
                CompanyId = info.CompanyId,
                TeamId = info.TeamId,
                TeamName = info.TeamName
            };
        }
    }
}
