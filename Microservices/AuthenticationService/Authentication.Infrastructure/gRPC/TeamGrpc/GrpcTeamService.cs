using Authentication.Core.DTOs;
using Authentication.Core.Interfaces;

namespace Authentication.Infrastructure.gRPC.TeamGrpc;

public class GrpcTeamService(TeamGrpcService.TeamGrpcServiceClient grpcServiceClient) : IGrpcTeamService
{
    public async Task<TeamDto> GetTeamByUserId(string userId)
    {
        var request = new GetUserTeamRequest { UserId = userId };
        var response = await grpcServiceClient.GetUserTeamAsync(request);
        return new TeamDto
        {
            CompanyId = response.CompanyId,
            TeamId = response.Id
        };
    }
}
