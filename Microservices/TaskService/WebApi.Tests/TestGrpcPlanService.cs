using Core.Dtos.Plan;
using Core.ServicesAbstractions;

namespace WebApi.Tests;
/// <summary>
/// Test implementation of IGrpcPlanService that always returns a valid plan
/// </summary>
public class TestGrpcPlanService : IGrpcPlanService
{
    public async Task<PlanIdsDto> GetPlanIdsAsync(string planId)
    {
        // Simulate async operation
        await Task.Delay(10);

        return new PlanIdsDto
        {
            IsExisted = true,
            PlanId = planId,
            CompanyId = "test-company",
            OwnerId = "test-user",
            TeamId = "test-team"
        };
    }
}
