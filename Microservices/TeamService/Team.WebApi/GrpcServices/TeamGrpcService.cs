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

        public override async Task<UserTeamInfoResponse> GetTeamByTeamLeader(
            GetUserTeamInfoRequest request,
            ServerCallContext context)
        {
            var info = await _teamService.GetTeamByTeamLeadIdAsync(request.UserId);

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

        public override async Task<UserExistResponse> UserExist(
            UserExistRequest request,
            ServerCallContext context)
        {
            var exist = await _teamService
                .IsEmployeeInTeamAsync(request.TeamId, request.UserId);

            return new UserExistResponse { Exist = exist };
        }
    }
}
