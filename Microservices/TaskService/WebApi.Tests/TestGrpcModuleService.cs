using Core.Dtos.Plan;
using Core.ServicesAbstractions;

namespace WebApi.Tests;

internal class TestGrpcModuleService : IGrpcModuleService
{
    public async Task<ModuleIdsDto> GetmoduleIdsAsync(string moduleId)
    {
        // Simulate async operation
        await Task.Delay(10);

        return new ModuleIdsDto
        {
            IsExisted = true,
            ModuleId = moduleId,
            CompanyId = "test-company",
            OwnerId = "test-user",
            TeamId = "test-team"
        };
    }
}
