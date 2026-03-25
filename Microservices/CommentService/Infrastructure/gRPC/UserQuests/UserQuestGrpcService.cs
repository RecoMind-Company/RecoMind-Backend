using Core.ServicesAbstraction;
using Microsoft.Extensions.Logging;

namespace Infrastructure.gRPC.UserQuests;

public class UserQuestGrpcService(GrpcUserQuestsService.GrpcUserQuestsServiceClient serviceClient, ILogger<UserQuestGrpcService> logger) : IUserQuestGrpcService
{
    public async Task<bool> IsUserInPlan(string userId, string planId)
    {
        var request = new IsUserInPlanRequest
        {
            UserId = userId,
            PlanId = planId
        };
        var response = await serviceClient.IsUserInAnyQuestWithinPlanAsync(request);
        return response.IsInPlan;
    }
}
