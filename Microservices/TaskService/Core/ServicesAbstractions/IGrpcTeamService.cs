namespace Core.ServicesAbstractions;

public interface IGrpcTeamService
{
    Task<bool> IsUserExist(string userId, string teamId);
}
