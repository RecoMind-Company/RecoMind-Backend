using Core.Dtos.AI;
using Core.ServicesAbstractions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace WebApi.gRPC.Quest;

public class QuestGrpcService(IQuestService questService) : GrpcQuestsService.GrpcQuestsServiceBase
{
    public override async Task<Empty> AddAIGeneratedTasks(ListOfAiTasksRequest request, ServerCallContext context)
    {
        IEnumerable<PostTasksDto> postTasksDtos = request.Aitasks.Select(protoTasks => new PostTasksDto
        {
            planId = request.PlanId,
            moduleId = protoTasks.ModuleId,
            tasksDto = protoTasks.Tasks.Select(protoTask => new AITasksDto
            {
                task_id = protoTask.TaskId,
                title = protoTask.Title,
                description = protoTask.Description,
                duration_days = protoTask.DurationDays,
                start_date = protoTask.StartDate,
                deadline_date = protoTask.DeadlineDate,
                status = protoTask.Status,
                priority = protoTask.Priority,

                suggested_owner = protoTask.SuggestedOwner != null ? new AISuggestedOwnersDto
                {
                    user_id = protoTask.SuggestedOwner.UserId,
                    job_title = protoTask.SuggestedOwner.JobTitle
                } : null
            }).ToList()
        }).ToList();

        await questService.AddAiTasksAsync(postTasksDtos);

        return new Empty();
    }
}
