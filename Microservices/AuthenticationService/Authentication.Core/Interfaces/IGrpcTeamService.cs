namespace Authentication.Core.Interfaces;

public interface IGrpcTeamService
{
    Task<string?> GetTeamByUserId(string userId);
}
