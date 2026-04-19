using Core.Dtos.Plan;

namespace Core.ServicesAbstraction;

public interface IGrpcPlanService
{
    Task<PlanIdsDto> GetPlanIdsAsync(string planId);
    Task<bool> IsOwnerOfPlanAsync(string userId, string planId);
}
