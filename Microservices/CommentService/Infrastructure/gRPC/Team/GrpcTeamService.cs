using Core.ServicesAbstraction;

namespace Infrastructure.gRPC.Team;

public class GrpcTeamService(TeamGrpcService.TeamGrpcServiceClient teamGrpcServiceClient) : IGrpcTeamService
{
    public async Task<bool> IsUserExist(string userId, string teamId)
    {
        var request = new UserExistRequest
        {
            UserId = userId,
            TeamId = teamId
        };

        var result = await teamGrpcServiceClient.UserExistAsync(request);

        return result.Exist;
    }
}
