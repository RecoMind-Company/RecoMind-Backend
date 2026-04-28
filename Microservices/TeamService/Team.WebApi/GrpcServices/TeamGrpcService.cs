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

        public override async Task<UserTeamInfoResponse> GetTeamByEmployee(
            GetUserTeamInfoRequest request,
            ServerCallContext context)
        {
            var result = await _teamService.GetTeamByEmployeeIdAsync(request.UserId);

            if (!result.IsSuccess)
                result = await _teamService.GetTeamByTeamLeadIdAsync(request.UserId);

            return result.Map(
                info => new UserTeamInfoResponse
                {
                    CompanyId = info.CompanyId,
                    TeamId = info.TeamId,
                    TeamName = info.TeamName
                },
                error => throw new RpcException(new Status(StatusCode.NotFound, error.Message))
            );
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
