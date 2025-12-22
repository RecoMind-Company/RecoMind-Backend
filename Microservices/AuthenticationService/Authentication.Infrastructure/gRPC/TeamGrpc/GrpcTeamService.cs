using Authentication.Core.Interfaces;
using Grpc.Core;

namespace Authentication.Infrastructure.gRPC.TeamGrpc;

public class GrpcTeamService(TeamGrpcService.TeamGrpcServiceClient grpcServiceClient) : IGrpcTeamService
{
    public async Task<string?> GetTeamByUserId(string userId)
    {
        var request = new GetUserTeamInfoRequest { UserId = userId };
        try
        {
            var response = await grpcServiceClient.GetTeamByTeamLeaderAsync(request);
            return response.CompanyId;
        }
        catch (RpcException ex)
        {
            return null;
        }

    }
}
