using Core.Dtos.Plan;
using Core.ServicesAbstractions;

namespace Infrastructure.gRPC.Plan;

public class GrpcPlanService(PlanServiceGrpc.PlanServiceGrpcClient planServiceGrpcClient) : IGrpcPlanService
{
    public async Task<PlanIdsDto> GetPlanIdsAsync(string planId)
    {
        var request = new PlanRequest { PlanId = planId };
        var result = await planServiceGrpcClient.GetPlanAsync(request);

        return new PlanIdsDto
        {
            IsExisted = result.IsExist,
            PlanId = result.PlanId,
            CompanyId = result.CompanyId,
            OwnerId = result.OwnerId,
            TeamId = result.TeamId
        };
    }
}
