
using Core.DTOs;

namespace Core.Interfaces;

public interface IGrpcTeamService
{
    Task<TeamToReturnDto> GetTeamByUserId(string userId);
}
