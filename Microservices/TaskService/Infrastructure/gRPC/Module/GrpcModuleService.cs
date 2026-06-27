using Core.Dtos.Plan;
using Core.ServicesAbstractions;

namespace Infrastructure.gRPC.Module;

public class GrpcModuleService(ModuleServiceGrpc.ModuleServiceGrpcClient moduleServiceGrpcClient) : IGrpcModuleService
{
    public async Task<ModuleIdsDto> GetmoduleIdsAsync(string moduleId)
    {
        var request = new ModuleRequest { ModuleId = moduleId };
        var response = await moduleServiceGrpcClient.GetModuleAsync(request);

        return new ModuleIdsDto
        {
            IsExisted = response.IsExist,
            ModuleId = response.ModuleId,
            CompanyId = response.CompanyId,
            TeamId = response.TeamId,
            OwnerId = response.OwnerId
        };
    }
}
