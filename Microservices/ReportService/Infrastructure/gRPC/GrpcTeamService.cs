
using Core.DTOs;
using Core.Interfaces;
using Grpc.Core;

namespace Infrastructure.gRPC;

public class GrpcTeamService(TeamGrpcService.TeamGrpcServiceClient grpcServiceClient) : IGrpcTeamService
{
    public async Task<TeamToReturnDto> GetTeamByUserId(string userId)
    {
        var request = new GetUserTeamInfoRequest { UserId = userId };
        try
        {
            var response = await grpcServiceClient.GetUserTeamInfoAsync(request);
            var teamToReturnDto = new TeamToReturnDto
            {
                TeamName = response.TeamName,
                TeamId = response.TeamId,
                CompanyId = response.CompanyId
            };
            return teamToReturnDto;
        }
        catch (RpcException ex)
        {
            return null;
        }
    }
}
