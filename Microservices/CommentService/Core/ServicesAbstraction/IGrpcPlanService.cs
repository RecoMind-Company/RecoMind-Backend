using Core.Dtos.Plan;

namespace Core.ServicesAbstraction;

public interface IGrpcPlanService
{
    Task<PlanIdsDto> GetPlanIdsAsync(string planId);
}
