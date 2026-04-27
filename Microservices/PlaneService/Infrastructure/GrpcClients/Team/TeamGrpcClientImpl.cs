using Core.Models;
using GrpcClients.Team;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.GrpcClients.Team
{
    public class TeamGrpcClientImpl(TeamGrpcService.TeamGrpcServiceClient _GrpcServiceClient) : ITeamGrpcClient
    {
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
