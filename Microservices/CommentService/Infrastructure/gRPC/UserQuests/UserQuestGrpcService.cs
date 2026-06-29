using Core.ServicesAbstraction;

namespace Infrastructure.gRPC.UserQuests;

public class UserQuestGrpcService(GrpcUserQuestsService.GrpcUserQuestsServiceClient serviceClient) : IUserQuestGrpcService
{
    public async Task<bool> IsUserAssignedToTask(string userId, string taskId)
    {
        var request = new IsUserAssignedToTaskRequest
        {
            UserId = userId,
            TaskId = taskId
        };
        var response = await serviceClient.IsUserAssignedToTaskAsync(request);
        return response.IsAssigned;
    }

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
