using Core.DTOs.Quest;

namespace Core.Interfaces;

public interface IQuestGrpcClient
{
    Task PostTasksToQuestService(PostTasksDto postTasksDtos);
}
