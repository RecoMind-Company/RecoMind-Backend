using Core.Models;

namespace Infrastructure.GrpcClients.Team
{
    public interface ITeamGrpcClient 
    {
        public Task<Result<string>> GetTeamNameById(string userId);
    }
}