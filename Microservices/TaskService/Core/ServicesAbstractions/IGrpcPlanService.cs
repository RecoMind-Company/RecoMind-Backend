using Core.Dtos.Plan;

namespace Core.ServicesAbstractions;

public interface IGrpcPlanService
{
    Task<PlanIdsDto> GetPlanIdsAsync(string planId);

}
