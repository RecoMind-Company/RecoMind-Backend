using Core.DTOs.TeamService;
using Core.Interfaces;
using Infrastructure.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class TeamService : ITeamService
    {
        private readonly TeamGrpcService.TeamGrpcServiceClient _grpcClient;
        public TeamService(TeamGrpcService.TeamGrpcServiceClient grpcClient)
        {
            _grpcClient = grpcClient;
        }
        public GetTeamInformationDto GetTeamInformation(string userId)
        {
            var result = _grpcClient.GetUserTeamInfo(new GetUserTeamInfoRequest { UserId = userId });

            return new GetTeamInformationDto { CompanyId = result.CompanyId , TeamName =result.TeamName };
        }
    }
}
