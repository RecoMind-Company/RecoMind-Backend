using Core.Dtos;
using Core.Result;

namespace Core.ServicesAbstractions;

public interface IUserQuestsService
{
    Task<Result<QuestToReturnDto>> AddUserToQuestAsync(AddUserToQuestDto userToQuestDto);
    Task<Result<IEnumerable<QuestToReturnDto>>> GetUserAssignedQuestsAsync(string userId);
    Task<Result<bool>> UnAssignUserFromQuestAsync(string questId, string userId);
}
