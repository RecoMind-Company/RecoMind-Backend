namespace Core.ServicesAbstraction;

public interface IGrpcTeamService
{
    Task<bool> IsUserExist(string userId, string teamId);
}
