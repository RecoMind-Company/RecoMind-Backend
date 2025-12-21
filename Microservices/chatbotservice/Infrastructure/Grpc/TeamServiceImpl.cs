using Core.DTOs.TeamService;
using Core.Interfaces;
using Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Grpc
{
    public class TeamServiceImpl(TeamGrpcService.TeamGrpcServiceClient _grpcClient) : ITeamService
    {
        public async Task<GetTeamInformationDto> GetTeamInformation(string userId)
        {
            try
            {
                var result = await _grpcClient.GetUserTeamInfoAsync(new GetUserTeamInfoRequest { UserId = userId });

                return new GetTeamInformationDto { CompanyId = result.CompanyId, TeamName = result.TeamName };
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
