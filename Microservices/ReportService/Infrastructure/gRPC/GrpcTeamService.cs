
using Core.DTOs;
using Core.Interfaces;

namespace Infrastructure.gRPC;

public class GrpcTeamService(TeamGrpcService.TeamGrpcServiceClient grpcServiceClient) : IGrpcTeamService
{
    public TeamToReturnDto GetTeamDetails(string teamId)
    {
        var request = new GetTeamByIdRequest { TeamId = teamId };
        var response = grpcServiceClient.GetTeamBasicInfo(request);
        var teamToReturnDto = new TeamToReturnDto
        {
            TeamName = response.Name,
            CompanyId = response.CompanyId
        };
        return teamToReturnDto;
    }
}
