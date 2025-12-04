
using Core.DTOs;

namespace Core.Interfaces;

public interface IGrpcTeamService
{
    TeamToReturnDto GetTeamDetails(string teamId);
}
