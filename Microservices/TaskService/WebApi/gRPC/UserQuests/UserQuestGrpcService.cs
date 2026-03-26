using Core.ServicesAbstractions;
using Grpc.Core;

namespace WebApi.gRPC.UserQuests;

public class UserQuestGrpcService(IUserQuestsService userQuestsService) : GrpcUserQuestsService.GrpcUserQuestsServiceBase
{
    public override async Task<IsUserInPlanResponse> IsUserInAnyQuestWithinPlan(IsUserInPlanRequest request, ServerCallContext context)
    {
        var isInPlan = await userQuestsService.IsUserAssignedToAnyQuestInPlan(request.UserId, request.PlanId);

        return new IsUserInPlanResponse { IsInPlan = isInPlan };
    }
}
