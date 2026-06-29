using Core.Dtos;
using Core.Dtos.AI;
using Core.Result;

namespace Core.ServicesAbstractions;

public interface IQuestService
{
    Task<Result<QuestToReturnDto>> CreateQuestAsync(QuestDto questDto);
    Task<Result<PersonalQuestToReturnDto>> CreatePersonalQuestAsync(FullQuestDto personalQuestDto);
    Task<Result<IEnumerable<QuestToReturnDto>>> GetAllQuestsAsync(string planId, string? moduleId);
    Task<Result<QuestToReturnDto>> GetQuestByIdAsync(string questId);
    Task<Result<QuestToReturnDto>> EditQuestAsync(UpdateQuestDto updateQuestDto, string questId);
    Task<Result<bool>> DeleteQuestAsync(string questId);
    Task<Result<IEnumerable<QuestToReturnDto>>> GetAllQuestsByStatusAsync(QuestByStatusDto questByStatusDto, string planId);
    Task AddAiTasksAsync(IEnumerable<PostTasksDto> postTasksDtos);
    Task<bool> IsTaskExists(string taskId);
    Task DeleteTasksByPlanId(string planId);
}
