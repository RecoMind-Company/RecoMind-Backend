using Core.Dtos.Plan;

namespace Core.ServicesAbstractions;

public interface IGrpcModuleService
{
    Task<ModuleIdsDto> GetmoduleIdsAsync(string moduleId);

}
