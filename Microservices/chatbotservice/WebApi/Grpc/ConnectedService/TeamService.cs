using Core.DTOs.TeamService;
using Team.Grpc;

namespace WebApi.Grpc.ConnectedService
{
    public class TeamService
    {
        private readonly TeamGrpcService.TeamGrpcServiceClient _teamServiceClient;

        public TeamService(TeamGrpcService.TeamGrpcServiceClient teamServiceClient)
        {
            _teamServiceClient = teamServiceClient;
        }
        public async Task<GetTeamInformationDto> GetTeamByUserId(string userId)
        {
            var request = new GetTeamByIdRequest
            {
                TeamId = userId
            };
            var response = await _teamServiceClient.GetTeamByUserIdAsync(request);
            if (response == null || string.IsNullOrEmpty(response.TeamName))
                return null;
            var teamInfoDto = new GetTeamInformationDto
            {
                TeamName = response.TeamName,
                CompanyId = response.CompanyId
            }

        public async Task<bool> CheckValidTeam(string userId)
        {
            var teamInfo = await GetTeamByUserId(userId);

            if (teamInfo == null)
                return false;
            return true;
        }
    }
}
