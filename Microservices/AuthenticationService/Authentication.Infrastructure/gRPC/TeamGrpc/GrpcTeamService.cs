using Authentication.Core.Interfaces;

namespace Authentication.Infrastructure.gRPC.TeamGrpc;

public class GrpcTeamService(TeamGrpcService.TeamGrpcServiceClient grpcServiceClient) : IGrpcTeamService
{
    //rpc GetTeamByEmployee(GetUserTeamInfoRequest) returns(UserTeamInfoResponse);

    /*
    message GetUserTeamInfoRequest {
        tring userId = 1;
    }

    message UserTeamInfoResponse {
      string companyId = 1;
      string teamId = 2;
      string teamName = 3;
    }
     */

    public async Task<string?> GetCompanyIdByUserId(string userId)
    {
        var request = new GetUserTeamInfoRequest { UserId = userId };

        var response = await grpcServiceClient.GetTeamByEmployeeAsync(request);

        return response?.CompanyId;
    }
}
