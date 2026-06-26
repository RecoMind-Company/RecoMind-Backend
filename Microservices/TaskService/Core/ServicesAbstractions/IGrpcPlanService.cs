using Core.Dtos.Plan;

namespace Core.ServicesAbstractions;

public interface IGrpcPlanService
{
    Task<ModuleIdsDto> GetmoduleIdsAsync(string planId);

}
