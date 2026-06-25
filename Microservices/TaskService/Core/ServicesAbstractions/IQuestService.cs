using Core.Dtos;
using Core.Result;

namespace Core.ServicesAbstractions;

public interface IQuestService
{
    Task<Result<QuestToReturnDto>> CreateQuestAsync(QuestDto questDto, string planId);
    Task<Result<PersonalQuestToReturnDto>> CreatePersonalQuestAsync(QuestDto personalQuestDto);
    Task<Result<IEnumerable<QuestToReturnDto>>> GetAllQuestsAsync(string planId);
    Task<Result<QuestToReturnDto>> EditQuestAsync(UpdateQuestDto updateQuestDto, string questId);
    Task<Result<bool>> DeleteQuestAsync(string questId);
    Task<Result<IEnumerable<QuestToReturnDto>>> GetAllQuestsByStatusAsync(QuestByStatusDto questByStatusDto, string planId);
}
