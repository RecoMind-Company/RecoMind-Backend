namespace Authentication.Core.Interfaces;

public interface IGrpcTeamService
{
    Task<string?> GetCompanyIdByUserId(string userId);
}
