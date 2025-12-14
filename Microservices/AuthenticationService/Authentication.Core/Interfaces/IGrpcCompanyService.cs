namespace Authentication.Core.Interfaces;

public interface IGrpcCompanyService
{
    Task<string> GetCompanyByUserId(string userId);
}
