using Core.Dtos;
using Core.Dtos.AI;
using Core.Result;

namespace Core.ServicesAbstractions;

public interface IQuestService
{
    Task<Result<QuestToReturnDto>> CreateQuestAsync(QuestDto questDto);
    Task<Result<PersonalQuestToReturnDto>> CreatePersonalQuestAsync(QuestDto personalQuestDto);
    Task<Result<IEnumerable<QuestToReturnDto>>> GetAllQuestsAsync(string moduleId);
    Task<Result<QuestToReturnDto>> EditQuestAsync(UpdateQuestDto updateQuestDto, string questId);
    Task<Result<bool>> DeleteQuestAsync(string questId);
    Task<Result<IEnumerable<QuestToReturnDto>>> GetAllQuestsByStatusAsync(QuestByStatusDto questByStatusDto, string moduleId);
    Task AddAiTasksAsync(IEnumerable<PostTasksDto> postTasksDtos);
}
