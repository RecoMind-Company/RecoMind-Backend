using Core.Dtos;
using Core.Result;

namespace Core.ServicesAbstractions;

public interface IQuestService
{
    Task<Result<QuestToReturnDto>> CreateQuestAsync(QuestDto questDto, string planId);
}
