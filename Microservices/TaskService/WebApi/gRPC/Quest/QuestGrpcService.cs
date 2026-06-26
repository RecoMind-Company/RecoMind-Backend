using Core.Dtos.AI;
using Core.ServicesAbstractions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace WebApi.gRPC.Quest;

public class QuestGrpcService(IQuestService questService) : GrpcQuestsService.GrpcQuestsServiceBase
{
    public override async Task<Empty> AddAIGeneratedTasks(AiTasks request, ServerCallContext context)
    {
        var tasks = request.Tasks.Select(t => new AITasksDto
        {
            task_id = t.TaskId,
            title = t.Title,
            description = t.Description,
            duration_days = t.DurationDays,
            start_date = t.StartDate,
            deadline_date = t.DeadlineDate,
            status = t.Status,
            priority = t.Priority,
            moduleId = t.ModuleId,
            suggested_owner = new AISuggestedOwnersDto
            {
                user_id = t.SuggestedOwner?.UserId!,
                job_title = t.SuggestedOwner?.JobTitle!
            }
        });
        await questService.AddAiTasksAsync(tasks);
        return new Empty();
    }
}
