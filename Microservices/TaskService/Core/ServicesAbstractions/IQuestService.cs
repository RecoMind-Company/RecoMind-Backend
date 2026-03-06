using Core.Dtos;
using Core.Result;

namespace Core.ServicesAbstractions;

public interface IQuestService
{
    Task<Result<QuestToReturnDto>> CreateQuestAsync(QuestDto questDto, string planId);
    Task<Result<IEnumerable<QuestToReturnDto>>> GetAllQuestsAsync(string planId);
    Task<Result<QuestToReturnDto>> EditQuestAsync(UpdateQuestDto updateQuestDto, string questId);
    Task<Result<QuestToReturnDto>> AddUserToQuestAsync(AddUserToQuestDto userToQuestDto);
    Task<Result<IEnumerable<QuestToReturnDto>>> GetUserAssignedQuestsAsync(string userId);
}
