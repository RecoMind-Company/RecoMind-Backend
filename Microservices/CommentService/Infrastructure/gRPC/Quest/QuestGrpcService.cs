using Core.ServicesAbstraction;
using Infrastructure.gRPC.Quest;

namespace WebApi.gRPC.Quest;

public class QuestGrpcService(GrpcQuestsService.GrpcQuestsServiceClient grpcQuestsServiceClient) : IQuestGrpcService
{
    public async Task<bool> IsTaskExists(string taskId)
    {
        var request = new IsTaskExistsRequest { TaskId = taskId };
        var response = await grpcQuestsServiceClient.IsTaskExistsAsync(request);
        return response.IsExists;
    }
}
