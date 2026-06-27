using Core.DTOs.Quest;
using Core.Interfaces;
using GrpcClients.Quest;

namespace Infrastructure.GrpcClients.Quest;

public class QuestGrpcClientImplementation(GrpcQuestsService.GrpcQuestsServiceClient grpcQuestsServiceClient) : IQuestGrpcClient
{
    public async Task PostTasksToQuestService(IEnumerable<PostTasksDto> postTasksDtos)
    {
        var request = new ListOfAiTasksRequest();

        request.Aitasks.AddRange(postTasksDtos.Select(dto => new AiTasks
        {
            ModuleId = dto.moduleId ?? string.Empty,
            Tasks =
            {
                dto.tasksDto?.Select(t => new AiTask
                {
                    TaskId = t.task_id ?? string.Empty,
                    Title = t.title ?? string.Empty,
                    Description = t.description ?? string.Empty,
                    DurationDays = t.duration_days,
                    StartDate = t.start_date ?? string.Empty,
                    DeadlineDate = t.deadline_date ?? string.Empty,
                    Status = t.status ?? string.Empty,
                    Priority = t.priority ?? string.Empty,
                    SuggestedOwner = t.suggested_owner != null ?

                    new AiSuggestedOwnersDto
                    {
                        UserId = t.suggested_owner.user_id ?? string.Empty,
                        JobTitle = t.suggested_owner.job_title ?? string.Empty
                    } : null
                }) ?? Enumerable.Empty<AiTask>()
            }
        }));

        await grpcQuestsServiceClient.AddAIGeneratedTasksAsync(request);
        // الآن الـ request جاهز تماماً لتبعتة من خلال الـ gRPC Client:
        // var response = await _grpcClient.YourMethodAsync(request);

    }
}
