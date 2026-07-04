using Core.Models;
using GrpcClients.Team;

namespace Infrastructure.GrpcClients.Team
{
    public class TeamGrpcClientImpl(TeamGrpcService.TeamGrpcServiceClient _GrpcServiceClient) : ITeamGrpcClient
    {
        public async Task<Result<string>> GetTeamLeaderId(string userId)
        {
            try
            {
                var result = await _GrpcServiceClient.GetTeamLeaderAsync(new TeamLeaderRequest { UserId = userId });
                if (result != null)
                {
                    return Result<string>.Success(result.LeaderId);
                }
                return Result<string>.Failure("No team leader found for the given user ID.");
            }
            catch (Exception)
            {
                return Result<string>.Failure(" Wrong ! Try Again Later ");
            }
        }

        public async Task<Result<string>> GetTeamNameById(string userId)
        {
            try
            {
                var result = await _GrpcServiceClient.GetTeamByEmployeeAsync(new GetUserTeamInfoRequest { UserId = userId });

                if (result != null)
                {
                    return Result<string>.Success(result.TeamId);
                }
                return Result<string>.Failure("No team found for the given user ID.");
            }
            catch (Exception ex)
            {
                return Result<string>.Failure(" Wrong ! Try Again Later ");
            }
        }

    }
}
