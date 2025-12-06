using Authentication.Core.DTOs;

namespace Authentication.Core.Interfaces;

public interface IGrpcTeamService
{
    Task<TeamDto> GetTeamByUserId(string userId);
}
